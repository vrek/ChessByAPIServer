using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.DTOs;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ChessByAPI.Tests;

public class GameControllerTests
{
    private readonly GameController _gameController;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ChessDbContext> _mockContext;

    public GameControllerTests()
    {
        // Set up the mock repository
        _gameRepositoryMock = new Mock<IGameRepository>();

        _ = _gameRepositoryMock.Setup(repo => repo.CreateGameAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Game(Guid.NewGuid(), 1, 2));


        // Create the controller and inject the mocked repository
        _gameController = new GameController(_gameRepositoryMock.Object);
        _mockContext = new Mock<ChessDbContext>();
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ChessDbContext(options);
    }


    // Test for GetGameById action
    [Fact]
    public async Task GetGameById_ReturnsOkResult_WithGame()
    {
        // Arrange: Set up a mock game
        var gameId = Guid.NewGuid();
        Game mockGame = new(gameId, 1, 2);
        _ = _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId))
            .ReturnsAsync(mockGame);

        // Act: Call the GetGameById action
        var result = await _gameController.GetGame(gameId);

        // Assert: Check if the result is OkObjectResult and contains the correct game
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedGame = Assert.IsType<Game>(okResult.Value);
        Assert.Equal(gameId, returnedGame.Id);
    }

    // Test for GetGameById action when game is not found
    [Fact]
    public async Task GetGameById_ReturnsNotFound_WhenGameDoesNotExist()
    {
        // Arrange: Set up the mock to return null
        var gameId = Guid.NewGuid();
        _ = _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId))
            .ReturnsAsync((Game)null);

        // Act: Call the GetGameById action
        var result = await _gameController.GetGame(gameId);

        // Assert: Check if the result is NotFoundResult
        _ = Assert.IsType<NotFoundResult?>(result.Result);
    }

    [Fact]
    public async Task CreateGame_ShouldReturnCreatedGame()
    {
        // Arrange
        var whitePlayerId = 1;
        var blackPlayerId = 2;
        var context = GetInMemoryDbContext();
        
        // Add mock users to the in-memory database
        await context.Users.AddRangeAsync(
            new User { Id = whitePlayerId, UserName = "White Player", Password = "Password", Email = "test@email.com"},
            new User { Id = blackPlayerId, UserName = "White Player", Password = "Password", Email = "test@email.com"}
        );
        await context.SaveChangesAsync();

        var gameRepository = new GameRepository(context);
        var gameController = new GameController(gameRepository);

        // Act
        var result = await gameController.CreateGame(whitePlayerId, blackPlayerId);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(gameController.GetGame), createdAtActionResult.ActionName);

        var createdGame = createdAtActionResult.Value as Game;
        Assert.NotNull(createdGame);
        Assert.Equal(whitePlayerId, createdGame.WhitePlayerId);
        Assert.Equal(blackPlayerId, createdGame.BlackPlayerId);

        // Verify that the game was actually added to the in-memory database
        var storedGame = await context.Games.FindAsync(createdGame.Id);
        Assert.NotNull(storedGame);
        Assert.Equal(whitePlayerId, storedGame.WhitePlayerId);
        Assert.Equal(blackPlayerId, storedGame.BlackPlayerId);
    }

    [Fact]
    public async Task CreateGameThrowsExceptionWithInvalidWhitePlayer()
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
        Task<List<UserDTO>> users = userRepository.GetAll();
        GameRepository gameRepository = new(_context);

        // Assert: Verify that an exception is thrown for an invalid white player
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            // Trying to create a game with invalid whitePlayerId (999)
            _ = await gameRepository.CreateGameAsync(999, 2); // user2 is valid
        });
    }

    [Fact]
    public async Task CreateGameThrowsExceptionWithInvalidBlackPlayer()
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
        _ = userRepository.GetAll();
        GameRepository gameRepository = new(_context);

        // Assert: Verify that an exception is thrown for an invalid white player
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            // Trying to create a game with invalid whitePlayerId (999)
            _ = await gameRepository.CreateGameAsync(1, 999); // user2 is valid
        });
    }

    [Fact]
    public async Task CreateGameCreatesGameWithValidPlayers()
    {
        // Arrange: Set up in-memory database context and repository
        var _context = GetInMemoryDbContext();
        UserRepository userRepository = new(_context);
        GameRepository gameRepo = new(_context);

        // Create two valid users
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

        // Add users to the context and save
        _ = await _context.Users.AddAsync(user1);
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();

        // Act: Create a game with valid players
        GameRepository gameRepository = new(_context);
        var createdGame = await gameRepository.CreateGameAsync(user1.Id, user2.Id);
        var gameresponse = gameRepo.CreateGameAsync(1, 2);

        // Assert: Check that the game was created successfully
        var retrievedGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == createdGame.Id);

        Assert.NotNull(retrievedGame); // Ensure the game exists
        Assert.Equal(user1.Id, retrievedGame.WhitePlayerId); // Check white player
        Assert.Equal(user2.Id, retrievedGame.BlackPlayerId); // Check black player
    }

    //[Fact]
    //public async Task CreateGameAsync_BothPlayersValid_CreatesGame()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;
    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = whitePlayerId, IsDeleted = false },
    //    new() { Id = blackPlayerId, IsDeleted = false }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    //    //_ = _mockContext.Setup(c => c.Games.AddAsync(It.IsAny<Game>(), default)).ReturnsAsync((Game g, CancellationToken ct) => g);
    //    _ = _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

    //    // Act
    //    var game = await _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId);

    //    // Assert
    //    Assert.NotNull(game);
    //    Assert.Equal(whitePlayerId, game.WhitePlayerId);
    //    Assert.Equal(blackPlayerId, game.BlackPlayerId);
    //}

    //[Fact]
    //public async Task CreateGameAsync_WhitePlayerDoesNotExist_ThrowsInvalidOperationException()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;

    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = blackPlayerId, IsDeleted = false }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    //        () => _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId)
    //    );
    //    Assert.Equal("White player with ID 1 does not exist.", exception.Message);
    //}

    //[Fact]
    //public async Task CreateGameAsync_BlackPlayerDoesNotExist_ThrowsInvalidOperationException()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;

    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = whitePlayerId, IsDeleted = false }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    //        () => _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId)
    //    );
    //    Assert.Equal("Black player with ID 2 does not exist.", exception.Message);
    //}

    //[Fact]
    //public async Task CreateGameAsync_WhitePlayerIsDeleted_ThrowsInvalidOperationException()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;

    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = whitePlayerId, IsDeleted = true },
    //    new() { Id = blackPlayerId, IsDeleted = false }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    //        () => _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId)
    //    );
    //    Assert.Equal("White player with ID 1 is deleted.", exception.Message);
    //}

    //[Fact]
    //public async Task CreateGameAsync_BlackPlayerIsDeleted_ThrowsInvalidOperationException()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;

    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = whitePlayerId, IsDeleted = false },
    //    new() { Id = blackPlayerId, IsDeleted = true }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

    //    // Act & Assert
    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    //        () => _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId)
    //    );
    //    Assert.Equal("Black player with ID 2 is deleted.", exception.Message);
    //}

    //[Fact]
    //public async Task CreateGameAsync_SaveChangesThrowsException_ThrowsException()
    //{
    //    // Arrange
    //    int whitePlayerId = 1;
    //    int blackPlayerId = 2;

    //    IQueryable<User> userList = new List<User>
    //{
    //    new() { Id = whitePlayerId, IsDeleted = false },
    //    new() { Id = blackPlayerId, IsDeleted = false }
    //}.AsQueryable();

    //    Mock<DbSet<User>> mockSet = new();
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
    //    _ = mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

    //    _ = _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    //    _mockContext.Setup(c => c.Games.AddAsync(It.IsAny<Game>(), default)).ReturnsAsync((Game g, CancellationToken ct) => g);
    //    _ = _mockContext.Setup(c => c.SaveChangesAsync(default)).ThrowsAsync(new Exception("Database error"));

    //    // Act & Assert
    //    await Assert.ThrowsAsync<Exception>(() => _gameRepositoryMock.CreateGameAsync(whitePlayerId, blackPlayerId));
    //}
}