using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChessByAPI.Tests;

public class UsersControllerTests
{
    private readonly UserController _controller;
    private readonly Mock<IUserRepository> _mockRepo;

    public UsersControllerTests()
    {
        // Create a mock IUserRepository
        _mockRepo = new Mock<IUserRepository>();

        // Inject the mock into the controller
        _controller = new UserController(_mockRepo.Object);
    }

    // Test adding a new user
    [Fact]
    public async Task AddUser_ShouldAddUserSuccessfully()
    {
        // Arrange
        User newUser = new()
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            Password = "password123"
        };

        // Mock the AddUser method to return the new user
        _ = _mockRepo.Setup(repo => repo.AddUser(It.IsAny<User>())).ReturnsAsync(newUser);

        // Act
        CreatedAtActionResult? result = await _controller.AddUser(newUser) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode); // Ensure it's Created (201)
        User? addedUser = result.Value as User;
        Assert.NotNull(addedUser);
        Assert.Equal("testuser", addedUser.UserName);
    }

    // Test adding a duplicate user (by email or username)
    [Fact]
    public async Task AddUser_ShouldReturnConflictIfUserExists()
    {
        // Arrange
        User newUser = new()
        {
            UserName = "existinguser",
            Email = "existinguser@example.com",
            Password = "password123"
        };

        // Mock the AddUser method to return null (indicating user already exists)
        _ = _mockRepo.Setup(repo => repo.AddUser(It.IsAny<User>())).ReturnsAsync((User?)null);

        // Act
        ConflictObjectResult? result = await _controller.AddUser(newUser) as ConflictObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(409, result.StatusCode); // Conflict (409)
    }

    // Test adding a user with invalid model
    [Fact]
    public async Task AddUser_ShouldReturnBadRequestForInvalidModel()
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
        BadRequestObjectResult? result = await _controller.AddUser(newUser) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode); // Bad Request (400)
    }

    [Fact]
    public async Task AddUser_ShouldReturnBadRequestForBlankEmail()
    {
        // Arrange
        User newUser = new()
        {
            UserName = "testuser",
            Email = "",
            Password = "password123"
        };

        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        BadRequestObjectResult? result = await _controller.AddUser(newUser) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode); // Bad Request (400)
    }
}

