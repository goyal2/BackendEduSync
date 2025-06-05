using Microsoft.AspNetCore.Mvc;
using EduSyncWebApi.Controllers;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class CourseModelsControllerTests : TestBase
    {
        private CourseModelsController _controller;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _controller = new CourseModelsController(_context);
        }

        [Test]
        public async Task GetCoursesByInstructor_WithoutInstructorId_ReturnsAllCourses()
        {
            // Arrange
            var courses = new List<CourseModel>
            {
                new CourseModel
                {
                    CourseId = Guid.NewGuid(),
                    Title = "Course 1",
                    Description = "Description 1",
                    InstructorId = Guid.NewGuid(),
                    MediaUrl = "url1"
                },
                new CourseModel
                {
                    CourseId = Guid.NewGuid(),
                    Title = "Course 2",
                    Description = "Description 2",
                    InstructorId = Guid.NewGuid(),
                    MediaUrl = "url2"
                }
            };
            await _context.CourseModels.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCoursesByInstructor(null);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCoursesByInstructor_WithInstructorId_ReturnsFilteredCourses()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var courses = new List<CourseModel>
            {
                new CourseModel
                {
                    CourseId = Guid.NewGuid(),
                    Title = "Course 1",
                    Description = "Description 1",
                    InstructorId = instructorId,
                    MediaUrl = "url1"
                },
                new CourseModel
                {
                    CourseId = Guid.NewGuid(),
                    Title = "Course 2",
                    Description = "Description 2",
                    InstructorId = Guid.NewGuid(),
                    MediaUrl = "url2"
                }
            };
            await _context.CourseModels.AddRangeAsync(courses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCoursesByInstructor(instructorId);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(1));
            Assert.That(result.Value.First().Title, Is.EqualTo("Course 1"));
        }

        [Test]
        public async Task GetCourseModel_WithValidId_ReturnsCourse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new CourseModel
            {
                CourseId = courseId,
                Title = "Test Course",
                Description = "Test Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "test-url"
            };
            await _context.CourseModels.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCourseModel(courseId);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.CourseId, Is.EqualTo(courseId));
            Assert.That(result.Value.Title, Is.EqualTo("Test Course"));
        }

        [Test]
        public async Task GetCourseModel_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var result = await _controller.GetCourseModel(invalidId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostCourse_WithValidData_CreatesCourse()
        {
            // Arrange
            var courseDto = new CourseDTO
            {
                CourseId = Guid.NewGuid(),
                Title = "New Course",
                Description = "New Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "new-url"
            };

            // Act
            var result = await _controller.PostCourse(courseDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            var createdCourse = createdResult.Value as CourseModel;
            Assert.That(createdCourse.Title, Is.EqualTo(courseDto.Title));
            Assert.That(createdCourse.Description, Is.EqualTo(courseDto.Description));
            Assert.That(createdCourse.MediaUrl, Is.EqualTo(courseDto.MediaUrl));
        }

        [Test]
        public async Task DeleteCourseModel_WithValidId_RemovesCourse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new CourseModel
            {
                CourseId = courseId,
                Title = "Test Course",
                Description = "Test Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "test-url"
            };
            await _context.CourseModels.AddAsync(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCourseModel(courseId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var deletedCourse = await _context.CourseModels.FindAsync(courseId);
            Assert.That(deletedCourse, Is.Null);
        }

        [Test]
        public async Task PutCourse_WithValidData_UpdatesCourse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new CourseModel
            {
                CourseId = courseId,
                Title = "Original Title",
                Description = "Original Description",
                InstructorId = Guid.NewGuid(),
                MediaUrl = "original-url"
            };
            await _context.CourseModels.AddAsync(course);
            await _context.SaveChangesAsync();

            var updatedCourseDto = new CourseDTO
            {
                CourseId = courseId,
                Title = "Updated Title",
                Description = "Updated Description",
                InstructorId = course.InstructorId,
                MediaUrl = "updated-url"
            };

            // Act
            var result = await _controller.PutCourse(courseId, updatedCourseDto);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var updatedCourse = await _context.CourseModels.FindAsync(courseId);
            Assert.That(updatedCourse.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedCourse.Description, Is.EqualTo("Updated Description"));
            Assert.That(updatedCourse.MediaUrl, Is.EqualTo("updated-url"));
        }

        [Test]
        public async Task PutCourse_WithMismatchedIds_ReturnsBadRequest()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var courseDto = new CourseDTO
            {
                CourseId = differentId,
                Title = "Test Course",
                Description = "Test Description"
            };

            // Act
            var result = await _controller.PutCourse(courseId, courseDto);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }
    }
}
