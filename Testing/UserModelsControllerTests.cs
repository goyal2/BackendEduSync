using Microsoft.AspNetCore.Mvc;
using EduSyncWebApi.Controllers;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;
using NUnit.Framework;

namespace Testing
{
    [TestFixture]
    public class UserModelsControllerTests : TestBase
    {
        private UserModelsController _controller;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _controller = new UserModelsController(_context);
        }

        [Test]
        public async Task GetUserModels_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<UserModel>
            {
                new UserModel 
                { 
                    UserId = Guid.NewGuid(), 
                    Name = "Test User 1", 
                    Email = "test1@example.com", 
                    Role = "Student",
                    PasswordHash = "hashedpassword1"
                },
                new UserModel 
                { 
                    UserId = Guid.NewGuid(), 
                    Name = "Test User 2", 
                    Email = "test2@example.com", 
                    Role = "Instructor",
                    PasswordHash = "hashedpassword2"
                }
            };
            await _context.UserModels.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUserModels();

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserModel_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserModel 
            { 
                UserId = userId, 
                Name = "Test User", 
                Email = "test@example.com", 
                Role = "Student",
                PasswordHash = "hashedpassword"
            };
            await _context.UserModels.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUserModel(userId);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.UserId, Is.EqualTo(userId));
            Assert.That(result.Value.Name, Is.EqualTo("Test User"));
        }

        [Test]
        public async Task GetUserModel_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUserModel(invalidId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostUserModel_WithValidData_CreatesUser()
        {
            // Arrange
            var userDto = new UserDTO
            {
                UserId = Guid.NewGuid(),
                Name = "New User",
                Email = "newuser@example.com",
                Role = "Student",
                PasswordHash = "hashedpassword"
            };

            // Act
            var result = await _controller.PostUserModel(userDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            var createdUser = createdResult.Value as UserModel;
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.Name, Is.EqualTo(userDto.Name));
            Assert.That(createdUser.Email, Is.EqualTo(userDto.Email));
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                Role = "Student",
                PasswordHash = "correctpassword"
            };
            await _context.UserModels.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginInput = new UserModelsController.LoginInput
            {
                Email = "test@example.com",
                PasswordHash = "correctpassword"
            };

            // Act
            var result = await _controller.Login(loginInput);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var returnedUser = okResult.Value as UserDTO;
            Assert.That(returnedUser.Email, Is.EqualTo(user.Email));
            Assert.That(returnedUser.PasswordHash, Is.Empty);
        }

        [Test]
        public async Task DeleteUserModel_WithValidId_RemovesUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserModel 
            { 
                UserId = userId, 
                Name = "Test User", 
                Email = "test@example.com", 
                Role = "Student",
                PasswordHash = "hashedpassword"
            };
            await _context.UserModels.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUserModel(userId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var deletedUser = await _context.UserModels.FindAsync(userId);
            Assert.That(deletedUser, Is.Null);
        }
    }
} 