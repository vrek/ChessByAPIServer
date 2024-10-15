using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPI.Tests
{
    public class UsersControllerTests
    {
        private readonly UserController _controller;
        private readonly ChessDbContext _context;

        private ChessDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ChessDbContext> options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ChessDbContext(options);
        }

        public UsersControllerTests()
        {
            // Create an in-memory database context

            _context = GetInMemoryDbContext();

            // Create the controller instance
            _controller = new UserController(_context);
        }

        // Test adding a new user
        [Fact]
        public void AddUser_ShouldAddUserSuccessfully()
        {
            // Arrange
            User newUser = new()
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "password123"
            };

            // Act
            CreatedAtActionResult? result = _controller.AddUser(newUser) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode); // Ensure it's Created (201)
            User? addedUser = result.Value as User;
            Assert.NotNull(addedUser);
            Assert.Equal("testuser", addedUser.UserName);

            // Ensure the user is actually in the database
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == "testuser@example.com");
            Assert.NotNull(userInDb);
            Assert.Equal("testuser", userInDb.UserName);
        }

        // Test adding a duplicate user (by email or username)
        [Fact]
        public void AddUser_ShouldReturnConflictIfUserExists()
        {
            // Arrange
            User existingUser = new()
            {
                UserName = "existinguser",
                Email = "existinguser@example.com",
                Password = "password123"
            };

            _ = _context.Users.Add(existingUser);
            _ = _context.SaveChanges();

            User newUser = new()
            {
                UserName = "existinguser", // Same username
                Email = "newemail@example.com",
                Password = "newpassword123"
            };

            // Act
            ConflictObjectResult? result = _controller.AddUser(newUser) as ConflictObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(409, result.StatusCode); // Conflict (409)
        }

        // Test adding a user with invalid model
        [Fact]
        public void AddUser_ShouldReturnBadRequestForInvalidModel()
        {
            // Arrange
            User newUser = new()
            {
                UserName = "", // Invalid empty username
                Email = "invalidemail",
                Password = "password123"
            };

            _controller.ModelState.AddModelError("UserName", "Username is required");

            // Act
            BadRequestObjectResult? result = _controller.AddUser(newUser) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode); // Bad Request (400)
        }
    }
}
