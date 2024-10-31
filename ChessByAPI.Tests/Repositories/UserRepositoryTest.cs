using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ChessByAPI.Tests.Repositories;

[TestSubject(typeof(UserRepository))]
public class UserRepositoryTest
{
    private readonly DbContext _context;
    private readonly UserController _controller;
    private readonly Mock<IUserRepository> _mockRepo;

    public UserRepositoryTest()
    {
        // Set up a new in-memory database for each test
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique name for isolation
            .Options;

        _context = new ChessDbContext(options);
        // Create a mock IUserRepository
        _mockRepo = new Mock<IUserRepository>();

        // Inject the mock into the controller
        _controller = new UserController(_mockRepo.Object);
    }

    public void Dispose()
    {
        // Teardown: Dispose of the context and database after each test
        _context.Dispose();
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ChessDbContext(options);
    }

    [Fact]
    public async Task GetByIDShouldReturnUserWithCorrectID()
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

        var result = userRepository.GetbyId(1).Result;
        Assert.Equal(user1.UserName, result.UserName);
        Assert.Equal(user1.Email, result.Email);
        Assert.Equal(user1.Password, result.Password);
        Assert.Equal(user1.Id, result.Id);
    }
}