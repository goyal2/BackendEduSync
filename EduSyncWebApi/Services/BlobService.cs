
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Azure;

namespace EduSyncWebApi.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<bool> TestConnectionAsync();
        Task<string> GetDetailedDiagnosticsAsync();
    }

    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobService> _logger;
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _logger = logger;

            _connectionString = configuration["AzureBlob:ConnectionString"];
            _containerName = configuration["AzureBlob:ContainerName"];

            _logger.LogInformation($"Initializing BlobService with container: {_containerName}");
            _logger.LogInformation($"Connection string length: {_connectionString?.Length ?? 0}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Azure Blob connection string is missing or empty");
                throw new InvalidOperationException("Azure Blob connection string is not configured");
            }

            if (string.IsNullOrEmpty(_containerName))
            {
                _logger.LogError("Azure Blob container name is missing or empty");
                throw new InvalidOperationException("Azure Blob container name is not configured");
            }

            try
            {
                _containerClient = new BlobContainerClient(_connectionString, _containerName);
                _logger.LogInformation("BlobContainerClient created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create BlobContainerClient");
                throw;
            }
        }

        public async Task<string> GetDetailedDiagnosticsAsync()
        {
            var diagnostics = new List<string>();

            try
            {
                diagnostics.Add($"Container Name: {_containerName ?? "NULL"}");
                diagnostics.Add($"Connection String Present: {!string.IsNullOrEmpty(_connectionString)}");
                diagnostics.Add($"Connection String Length: {_connectionString?.Length ?? 0}");

                // Test if container exists
                _logger.LogInformation("Testing container existence...");
                var containerExists = await _containerClient.ExistsAsync();
                diagnostics.Add($"Container Exists: {containerExists.Value}");

                if (!containerExists.Value)
                {
                    _logger.LogInformation("Container doesn't exist, attempting to create...");
                    var createResponse = await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                    diagnostics.Add($"Container Created: {createResponse != null}");
                }

                // Test container properties
                _logger.LogInformation("Getting container properties...");
                var properties = await _containerClient.GetPropertiesAsync();
                diagnostics.Add($"Container Properties Retrieved: {properties != null}");
                diagnostics.Add($"Container URI: {_containerClient.Uri}");

                // List existing blobs (first 10)
                _logger.LogInformation("Listing existing blobs...");
                var blobs = new List<string>();
                var blobCount = 0;
                await foreach (var blob in _containerClient.GetBlobsAsync())
                {
                    if (blobCount >= 10) break;
                    blobs.Add(blob.Name);
                    blobCount++;
                }
                diagnostics.Add($"Existing Blobs Count: {blobs.Count}");
                if (blobs.Count > 0)
                {
                    diagnostics.Add($"Sample Blobs: {string.Join(", ", blobs)}");
                }

                return string.Join("\n", diagnostics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during diagnostics");
                diagnostics.Add($"Error: {ex.Message}");
                if (ex.StackTrace != null)
                {
                    diagnostics.Add($"Stack Trace: {ex.StackTrace}");
                }
                return string.Join("\n", diagnostics);
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Starting connection test...");

                // Step 1: Check if container exists
                var existsResponse = await _containerClient.ExistsAsync();
                _logger.LogInformation($"Container exists check: {existsResponse.Value}");

                if (!existsResponse.Value)
                {
                    _logger.LogInformation("Container doesn't exist, creating...");
                    var createResponse = await _containerClient.CreateAsync(PublicAccessType.Blob);
                    _logger.LogInformation($"Container created with status: {createResponse.GetRawResponse().Status}");
                }

                // Step 2: Try to get container properties
                var properties = await _containerClient.GetPropertiesAsync();
                _logger.LogInformation($"Retrieved container properties: {properties.Value.LastModified}");

                // Step 3: Try to upload a test blob
                var testBlobName = $"test-connection-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
                var testContent = "Connection test successful";
                var testBlob = _containerClient.GetBlobClient(testBlobName);

                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
                await testBlob.UploadAsync(stream, overwrite: true);
                _logger.LogInformation($"Test blob uploaded: {testBlob.Uri}");

                // Step 4: Verify the blob exists
                var blobExists = await testBlob.ExistsAsync();
                _logger.LogInformation($"Test blob exists: {blobExists.Value}");

                // Step 5: Clean up test blob
                await testBlob.DeleteIfExistsAsync();
                _logger.LogInformation("Test blob deleted");

                return blobExists.Value;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, $"Azure Storage request failed: Status={ex.Status}, ErrorCode={ex.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed with unexpected error");
                return false;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                _logger.LogInformation($"Starting upload for file: {fileName}, Size: {fileStream.Length} bytes");

                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new ArgumentException("File stream is null or empty");
                }

                // Reset stream position
                fileStream.Position = 0;
                _logger.LogInformation($"Stream position reset to: {fileStream.Position}");

                // Generate unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var uniqueFileName = $"{timestamp}_{fileName}";
                _logger.LogInformation($"Generated unique filename: {uniqueFileName}");

                var blobClient = _containerClient.GetBlobClient(uniqueFileName);
                _logger.LogInformation($"BlobClient created for: {blobClient.Uri}");

                // Ensure container exists
                await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                _logger.LogInformation("Container existence ensured");

                // Set content type
                var contentType = GetContentType(fileName);
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                _logger.LogInformation($"Content type set to: {contentType}");

                // Upload options
                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = new Dictionary<string, string>
                    {
                        { "OriginalFileName", fileName },
                        { "UploadedAt", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                        { "FileSize", fileStream.Length.ToString() }
                    }
                };

                _logger.LogInformation("Starting blob upload...");
                var uploadResponse = await blobClient.UploadAsync(fileStream, blobUploadOptions);
                _logger.LogInformation($"Upload completed with status: {uploadResponse.GetRawResponse().Status}");

                // Verify the blob was created
                var blobExists = await blobClient.ExistsAsync();
                _logger.LogInformation($"Blob exists after upload: {blobExists.Value}");

                if (!blobExists.Value)
                {
                    throw new InvalidOperationException("Blob upload appeared successful but blob doesn't exist");
                }

                var blobUrl = blobClient.Uri.ToString();
                _logger.LogInformation($"File uploaded successfully: {blobUrl}");

                return blobUrl;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, $"Azure Storage request failed during upload: Status={ex.Status}, ErrorCode={ex.ErrorCode}, Message={ex.Message}");
                throw new InvalidOperationException($"Azure Storage error: {ex.ErrorCode} - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload file: {fileName}");
                throw;
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}


//using Azure.Storage.Blobs;

//namespace EduSyncWebApi.Services
//{
//    public interface IBlobService
//    {
//        Task<string> UploadFileAsync(Stream fileStream, string fileName);
//    }

//    public class BlobService : IBlobService
//    {
//        private readonly BlobContainerClient _containerClient;

//        public BlobService(IConfiguration configuration)
//        {
//            var connectionString = configuration["AzureBlob:ConnectionString"];
//            var containerName = configuration["AzureBlob:ContainerName"];
//            _containerClient = new BlobContainerClient(connectionString, containerName);
//            _containerClient.CreateIfNotExists();
//        }

//        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
//        {
//            var blobClient = _containerClient.GetBlobClient(fileName);
//            await blobClient.UploadAsync(fileStream, overwrite: true);
//            return blobClient.Uri.ToString();
//        }
//    }
//}
