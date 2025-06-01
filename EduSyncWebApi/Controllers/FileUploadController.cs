using Microsoft.AspNetCore.Mvc;
using EduSyncWebApi.Services;

namespace EduSyncWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IBlobService _blobService;
        private readonly ILogger<FileUploadController> _logger;
        private readonly IConfiguration _configuration;

        public FileUploadController(IBlobService blobService, ILogger<FileUploadController> logger, IConfiguration configuration)
        {
            _blobService = blobService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("diagnostics")]
        public async Task<IActionResult> GetDiagnostics()
        {
            try
            {
                var diagnostics = await _blobService.GetDetailedDiagnosticsAsync();

                var configInfo = new
                {
                    ConnectionStringConfigured = !string.IsNullOrEmpty(_configuration["AzureBlob:ConnectionString"]),
                    ContainerNameConfigured = !string.IsNullOrEmpty(_configuration["AzureBlob:ContainerName"]),
                    ConnectionStringLength = _configuration["AzureBlob:ConnectionString"]?.Length ?? 0,
                    ContainerName = _configuration["AzureBlob:ContainerName"],
                    AccountName = _configuration["AzureBlob:AccountName"]
                };

                return Ok(new
                {
                    Configuration = configInfo,
                    BlobDiagnostics = diagnostics,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diagnostics");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("Testing blob storage connection...");
                var isConnected = await _blobService.TestConnectionAsync();

                return Ok(new
                {
                    connected = isConnected,
                    timestamp = DateTime.UtcNow,
                    message = isConnected ? "Connection successful" : "Connection failed - check logs for details"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return Ok(new
                {
                    connected = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                _logger.LogInformation($"=== UPLOAD REQUEST START ===");
                _logger.LogInformation($"File received: {file?.FileName}");
                _logger.LogInformation($"File size: {file?.Length ?? 0} bytes");
                _logger.LogInformation($"Content type: {file?.ContentType}");

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload request with no file or empty file");
                    return BadRequest(new
                    {
                        success = false,
                        error = "No file uploaded or file is empty.",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Log request details
                _logger.LogInformation($"Request Content-Type: {Request.ContentType}");
                _logger.LogInformation($"Request Method: {Request.Method}");
                _logger.LogInformation($"Request Path: {Request.Path}");

                // Validate file size (100MB limit)
                const long maxFileSize = 100 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"File too large: {file.Length} bytes (max: {maxFileSize})");
                    return BadRequest(new
                    {
                        success = false,
                        error = "File size exceeds maximum limit of 100MB.",
                        fileSize = file.Length,
                        maxSize = maxFileSize,
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Opening file stream...");
                using var stream = file.OpenReadStream();
                _logger.LogInformation($"Stream opened. Length: {stream.Length}, Position: {stream.Position}");

                _logger.LogInformation("Calling blob service upload...");
                var url = await _blobService.UploadFileAsync(stream, file.FileName);

                _logger.LogInformation($"=== UPLOAD SUCCESS ===");
                _logger.LogInformation($"Final URL: {url}");

                return Ok(new
                {
                    success = true,
                    url = url,
                    fileName = file.FileName,
                    fileSize = file.Length,
                    contentType = file.ContentType,
                    timestamp = DateTime.UtcNow,
                    message = "File uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== UPLOAD FAILED ===");
                _logger.LogError($"Error Type: {ex.GetType().Name}");
                _logger.LogError($"Error Message: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    errorType = ex.GetType().Name,
                    timestamp = DateTime.UtcNow,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("test-upload")]
        public async Task<IActionResult> TestUpload()
        {
            try
            {
                _logger.LogInformation("Creating test file for upload...");

                var testContent = $"Test file created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
                var testFileName = $"test-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));

                _logger.LogInformation($"Uploading test file: {testFileName}");
                var url = await _blobService.UploadFileAsync(stream, testFileName);

                return Ok(new
                {
                    success = true,
                    url = url,
                    fileName = testFileName,
                    content = testContent,
                    timestamp = DateTime.UtcNow,
                    message = "Test file uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test upload failed");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using EduSyncWebApi.Services;

//namespace EduSyncWebApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FileUploadController : ControllerBase
//    {
//        private readonly IBlobService _blobService;

//        public FileUploadController(IBlobService blobService)
//        {
//            _blobService = blobService;
//        }

//        [HttpPost]
//        public async Task<IActionResult> UploadFile(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded.");

//            var url = await _blobService.UploadFileAsync(file.OpenReadStream(), file.FileName);
//            return Ok(new { url });
//        }
//    }
//}
