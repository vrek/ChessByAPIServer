using ChessByAPIServer;
using ChessByAPIServer.Contexts;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ChessByAPI.Tests.Repositories;

public class UsersControllerTests
{
    private readonly ChessDbContext _context;
    private readonly UserController _controller;
    private readonly Mock<IUserRepository> _mockRepo;

    public UsersControllerTests()
    {
        // Set up a new in-memory database for each test
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique name for isolation
            .Options;

        _context = new ChessDbContext(options);

        _mockRepo = new Mock<IUserRepository>();

        // Inject the mock into the controller
        _controller = new UserController(_mockRepo.Object, _context);
    }

    public void Dispose()
    {
        // Teardown: Dispose of the context and database after each test
        _context.Dispose();
    }

    // Test adding a new user
    [Fact]
    public async Task AddUser_ShouldAddUserSuccessfully()
    {
        // Arrange
        User? newUser = new()
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            Password = "password123"
        };

        // Mock the AddUser method to return the new user
        _ = _mockRepo.Setup(repo => repo.AddUser(It.IsAny<User>())).ReturnsAsync(newUser);

        // Act
        var result = await _controller.AddUser(newUser) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode); // Ensure it's Created (201)
        var addedUser = result.Value as User;
        Assert.NotNull(addedUser);
        Assert.Equal("testuser", addedUser.UserName);
    }

    // Test adding a duplicate user (by email or username)
    [Fact]
    public async Task AddUser_ShouldReturnConflictIfUserExists()
    {
        // Arrange
        User? newUser = new()
        {
            UserName = "existinguser",
            Email = "existinguser@example.com",
            Password = "password123"
        };

        // Mock the AddUser method to return null (indicating user already exists)
        _ = _mockRepo.Setup(repo => repo.AddUser(It.IsAny<User>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.AddUser(newUser) as ConflictObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(409, result.StatusCode); // Conflict (409)
    }

    // Test adding a user with invalid model
    [Fact]
    public async Task AddUser_ShouldReturnBadRequestForInvalidModel()
    {
        // Arrange
        User? newUser = new()
        {
            UserName = "", // Invalid empty username
            Email = "invalidemail",
            Password = "password123"
        };

        _controller.ModelState.AddModelError("UserName", "Username is required");

        // Act
        var result = await _controller.AddUser(newUser) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode); // Bad Request (400)
    }

    [Fact]
    public async Task AddUser_ShouldReturnBadRequestForBlankEmail()
    {
        // Arrange
        User? newUser = new()
        {
            UserName = "testuser",
            Email = "",
            Password = "password123"
        };

        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.AddUser(newUser) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode); // Bad Request (400)
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ChessDbContext(options);
    }

    [Fact]
    public async Task GivenDBof2UsersShouldGetAllReturnsListof2users()
    {
        var _context = GetInMemoryDbContext();
        UserRepository userRepository = new(_context);
        User? user1 = new()
        {
            UserName = "testuser",
            Email = "email1@test.com",
            Password = "password123"
        };
        User? user2 = new()
        {
            UserName = "testuser2",
            Email = "email2@test.com",
            Password = "password1234"
        };

        _ = await _context.Users.AddAsync(user1);
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();
        Task<List<UserDto>> users = userRepository.GetAll();
        var Expected = 2;
        var result = users.Result.Count();
        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task GetbyEmailShouldReturnIDofUserAssociatedWithEmail()
    {
        var _context = GetInMemoryDbContext();
        UserRepository userRepository = new(_context);
        User? user1 = new()
        {
            UserName = "testuser",
            Email = "email1@test.com",
            Password = "password123"
        };
        User? user2 = new()
        {
            UserName = "testuser2",
            Email = "email2@test.com",
            Password = "password1234"
        };

        _ = await _context.Users.AddAsync(user1);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();

        var userId = userRepository.GetbyEmail("email2@test.com");
        var Expected = user2.Id;
        var result = userId.Result;
        Assert.Equal(Expected, result);
    }
}