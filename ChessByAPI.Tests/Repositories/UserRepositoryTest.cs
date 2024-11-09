
using ChessByAPIServer.Contexts;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;


namespace ChessByAPI.Tests.Repositories;

[TestSubject(typeof(UserRepository))]
public class UserRepositoryTest
{
    private readonly ChessDbContext _context;
    private readonly UserController _controller;
    private readonly UserRepository _repo;

    public UserRepositoryTest()
    {
        // Set up a new in-memory database for each test
        _context = GetInMemoryDbContext();
        // Create a mock IUserRepository
        _repo = new(_context);

        // Inject the mock into the controller
        _controller = new UserController(_repo, _context);
    }

    [Fact]
    public void Dispose()
    {
        // Teardown: Dispose of the context and database after each test
        _context.Dispose();
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var _options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ChessDbContext(_options);
    }

    [Theory]
    [InlineData(1, "testuser", "email1@test.com", "password123", true)] // Valid existing user (user1)
    [InlineData(2, "testuser2", "email2@test.com", "password1234", true)] // Valid existing user (user2)
    [InlineData(-1, null, null, null, false)] // Edge case: Invalid negative ID
    [InlineData(999, null, null, null, false)] // Edge case: Non-existent ID
    [InlineData(int.MaxValue, null, null, null, false)] // Edge case: ID with Max Int value
    [InlineData(int.MinValue, null, null, null, false)] // Edge case: ID with Min Int value
    public async Task GetById_ShouldReturnExpectedResult(
        int userId,
        string expectedUserName,
        string expectedEmail,
        string expectedPassword,
        bool shouldExist
    )
    {
        // Arrange
        User _user1 = new()
        {
            UserName = "testuser",
            Email = "email1@test.com",
            Password = "password123"
        };
        User _user2 = new()
        {
            UserName = "testuser2",
            Email = "email2@test.com",
            Password = "password1234"
        };

        // Add users to in-memory database
        await _context.Users.AddAsync(_user1);
        await _context.SaveChangesAsync();
        await _context.Users.AddAsync(_user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetbyIdAsync(userId);

        // Assert
        if (shouldExist)
        {
            Assert.NotNull(result);
            Assert.Equal(expectedUserName, result.UserName);
            Assert.Equal(expectedEmail, result.Email);
            Assert.Equal(expectedPassword, result.Password);
        }
        else
        {
            Assert.Null(result); // For non-existent or invalid user IDs
        }
    }

    [Fact]

public async Task GetById_ShouldReturnFalseIfUserIsDeleted()
    {
        // Arrange
        User _user1 = new()
        {
            UserName = "testuser",
            Email = "email1@test.com",
            Password = "password123"
        };
        User _user2 = new()
        {
            UserName = "testuser2",
            Email = "email2@test.com",
            Password = "password1234"
        };
        User _user3 = new()
        {
            UserName = "testuser3",
            Email = "email3@test.com",
            Password = "password134"
        };

        // Add users to in-memory database
        await _context.Users.AddAsync(_user1);
        await _context.SaveChangesAsync();
        await _context.Users.AddAsync(_user2);
        await _context.SaveChangesAsync();
        await _context.Users.AddAsync(_user3);
        await _context.SaveChangesAsync();

        // Act
        await _repo.DeleteUser(3);
        var result = await _repo.GetbyIdAsync(3);

        // Assert
        Assert.Null(result); // For non-existent or invalid user IDs
    }

}