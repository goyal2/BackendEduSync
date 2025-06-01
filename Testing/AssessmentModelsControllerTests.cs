using Microsoft.AspNetCore.Mvc;
using EduSyncWebApi.Controllers;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class AssessmentModelsControllerTests : TestBase
    {
        private AssessmentModelsController _controller;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _controller = new AssessmentModelsController(_context);
        }

        [Test]
        public async Task GetAssessmentModels_ReturnsAllAssessments()
        {
            // Arrange
            var assessments = new List<AssessmentModel>
            {
                new AssessmentModel { AssessmentId = Guid.NewGuid(), Title = "Assessment 1", Questions = "Q1", MaxScore = 100 },
                new AssessmentModel { AssessmentId = Guid.NewGuid(), Title = "Assessment 2", Questions = "Q2", MaxScore = 50 }
            };
            await _context.AssessmentModels.AddRangeAsync(assessments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAssessmentModels();

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAssessmentModel_WithValidId_ReturnsAssessment()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            var assessment = new AssessmentModel 
            { 
                AssessmentId = assessmentId, 
                Title = "Test Assessment", 
                Questions = "Test Questions",
                MaxScore = 100 
            };
            await _context.AssessmentModels.AddAsync(assessment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAssessmentModel(assessmentId);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.AssessmentId, Is.EqualTo(assessmentId));
            Assert.That(result.Value.Title, Is.EqualTo("Test Assessment"));
        }

        [Test]
        public async Task PostAssessment_WithValidData_CreatesAssessment()
        {
            // Arrange
            var assessmentDto = new AssessmentDTO
            {
                AssessmentId = Guid.NewGuid(),
                Title = "New Assessment",
                Questions = "New Questions",
                MaxScore = 100,
                CourseId = Guid.NewGuid()
            };

            // Act
            var result = await _controller.PostAssessment(assessmentDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var createdAssessment = createdResult.Value as AssessmentModel;
            Assert.That(createdAssessment.Title, Is.EqualTo(assessmentDto.Title));
            Assert.That(createdAssessment.Questions, Is.EqualTo(assessmentDto.Questions));
            Assert.That(createdAssessment.MaxScore, Is.EqualTo(assessmentDto.MaxScore));
        }

        [Test]
        public async Task PutAssessment_WithValidData_UpdatesAssessment()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            var assessment = new AssessmentModel 
            { 
                AssessmentId = assessmentId, 
                Title = "Original Title", 
                Questions = "Original Questions",
                MaxScore = 100 
            };
            await _context.AssessmentModels.AddAsync(assessment);
            await _context.SaveChangesAsync();

            var updatedAssessmentDto = new AssessmentDTO
            {
                AssessmentId = assessmentId,
                Title = "Updated Title",
                Questions = "Updated Questions",
                MaxScore = 150
            };

            // Act
            var result = await _controller.PutAssessment(assessmentId, updatedAssessmentDto);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var updatedAssessment = await _context.AssessmentModels.FindAsync(assessmentId);
            Assert.That(updatedAssessment.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedAssessment.Questions, Is.EqualTo("Updated Questions"));
            Assert.That(updatedAssessment.MaxScore, Is.EqualTo(150));
        }

        [Test]
        public async Task DeleteAssessmentModel_WithValidId_RemovesAssessment()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            var assessment = new AssessmentModel 
            { 
                AssessmentId = assessmentId, 
                Title = "Test Assessment", 
                Questions = "Test Questions",
                MaxScore = 100 
            };
            await _context.AssessmentModels.AddAsync(assessment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAssessmentModel(assessmentId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var deletedAssessment = await _context.AssessmentModels.FindAsync(assessmentId);
            Assert.That(deletedAssessment, Is.Null);
        }

        [Test]
        public async Task PutAssessment_WithMismatchedIds_ReturnsBadRequest()
        {
            // Arrange
            var assessmentId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var assessmentDto = new AssessmentDTO
            {
                AssessmentId = differentId,
                Title = "Test Assessment",
                Questions = "Test Questions",
                MaxScore = 100
            };

            // Act
            var result = await _controller.PutAssessment(assessmentId, assessmentDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }
    }
} 