using Microsoft.AspNetCore.Mvc;
using EduSyncWebApi.Controllers;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class ResultModelsControllerTests : TestBase
    {
        private ResultModelsController _controller;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _controller = new ResultModelsController(_context);
        }

        [Test]
        public async Task GetResultModels_ReturnsAllResults()
        {
            // Arrange
            var results = new List<ResultModel>
            {
                new ResultModel 
                { 
                    ResultId = Guid.NewGuid(), 
                    Score = 85,
                    AssessmentId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    AttemptDate = DateTime.Now
                },
                new ResultModel 
                { 
                    ResultId = Guid.NewGuid(), 
                    Score = 45,
                    AssessmentId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    AttemptDate = DateTime.Now
                }
            };
            await _context.ResultModels.AddRangeAsync(results);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetResultModels();

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetResultModel_WithValidId_ReturnsResult()
        {
            // Arrange
            var resultId = Guid.NewGuid();
            var resultModel = new ResultModel 
            { 
                ResultId = resultId, 
                Score = 90,
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AttemptDate = DateTime.Now
            };
            await _context.ResultModels.AddAsync(resultModel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetResultModel(resultId);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.ResultId, Is.EqualTo(resultId));
            Assert.That(result.Value.Score, Is.EqualTo(90));
        }

        [Test]
        public async Task PostResult_WithValidData_CreatesResult()
        {
            // Arrange
            var resultDto = new ResultDTO
            {
                ResultId = Guid.NewGuid(),
                Score = 75,
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AttemptDate = DateTime.Now
            };

            // Act
            var result = await _controller.PostResult(resultDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            var createdResultModel = createdResult.Value as ResultModel;
            Assert.That(createdResultModel, Is.Not.Null);
            Assert.That(createdResultModel.Score, Is.EqualTo(resultDto.Score));
        }

        [Test]
        public async Task PutResult_WithValidData_UpdatesResult()
        {
            // Arrange
            var resultId = Guid.NewGuid();
            var resultModel = new ResultModel 
            { 
                ResultId = resultId, 
                Score = 80,
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AttemptDate = DateTime.Now
            };
            await _context.ResultModels.AddAsync(resultModel);
            await _context.SaveChangesAsync();

            var updatedResultDto = new ResultDTO
            {
                ResultId = resultId,
                Score = 95,
                AssessmentId = resultModel.AssessmentId,
                UserId = resultModel.UserId,
                AttemptDate = resultModel.AttemptDate
            };

            // Act
            var result = await _controller.PutResult(resultId, updatedResultDto);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var updatedResult = await _context.ResultModels.FindAsync(resultId);
            Assert.That(updatedResult, Is.Not.Null);
            Assert.That(updatedResult.Score, Is.EqualTo(95));
        }

        [Test]
        public async Task DeleteResultModel_WithValidId_RemovesResult()
        {
            // Arrange
            var resultId = Guid.NewGuid();
            var resultModel = new ResultModel 
            { 
                ResultId = resultId, 
                Score = 85,
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AttemptDate = DateTime.Now
            };
            await _context.ResultModels.AddAsync(resultModel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteResultModel(resultId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var deletedResult = await _context.ResultModels.FindAsync(resultId);
            Assert.That(deletedResult, Is.Null);
        }

        [Test]
        public async Task GetResultModel_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var result = await _controller.GetResultModel(invalidId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PutResult_WithMismatchedIds_ReturnsBadRequest()
        {
            // Arrange
            var resultId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var resultDto = new ResultDTO
            {
                ResultId = differentId,
                Score = 85,
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AttemptDate = DateTime.Now
            };

            // Act
            var result = await _controller.PutResult(resultId, resultDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }
    }
} 