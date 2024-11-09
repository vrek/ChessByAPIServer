using ChessByAPIServer;
using ChessByAPIServer.Contexts;
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
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IChessBoardRepository> _chessBoardRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMoveRepository> _moveRepositoryMock;
    private readonly GameController _gameController;

    public GameControllerTests()
    {
        // Set up the mock repositories
        _gameRepositoryMock = new Mock<IGameRepository>();
        _chessBoardRepositoryMock = new Mock<IChessBoardRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _moveRepositoryMock = new Mock<IMoveRepository>(); // Example setup for _gameRepositoryMock 
        _gameRepositoryMock.Setup(repo => repo.CreateGameAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Game(Guid.NewGuid(), 1,
                2));
        // Inject the mocked repositories into the controller
        _gameController = new GameController(_gameRepositoryMock.Object, _chessBoardRepositoryMock.Object,
            _userRepositoryMock.Object, _moveRepositoryMock.Object);
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

    [Theory]
    [InlineData(1, 2, true, null)]       // Valid players, expecting a successful game creation
    [InlineData(999, 2, false, typeof(InvalidOperationException))] // Invalid white player, expecting InvalidOperationException
    [InlineData(1, 999, false, typeof(InvalidOperationException))] // Invalid black player, expecting InvalidOperationException
    public async Task CreateGame_VariousScenarios(
        int whitePlayerId,
        int blackPlayerId,
        bool shouldCreateGame,
        Type? expectedExceptionType)
    {
        // Arrange
        var context = GetInMemoryDbContext();
        await context.Users.AddRangeAsync(
            new User { Id = 1, UserName = "testuser1", Email = "email1@test.com", Password = "password123" },
            new User { Id = 2, UserName = "testuser2", Email = "email2@test.com", Password = "password1234" }
        );
        await context.SaveChangesAsync();
        var gameRepository = new GameRepository(context);

        // Act & Assert
        if (expectedExceptionType != null)
        {
            // If an exception is expected, assert that it is thrown
            await Assert.ThrowsAsync(expectedExceptionType, async () =>
            {
                await gameRepository.CreateGameAsync(whitePlayerId, blackPlayerId);
            });
        }
        else
        {
            // Otherwise, expect a successful game creation
            var createdGame = await gameRepository.CreateGameAsync(whitePlayerId, blackPlayerId);

            // Verify the game was created as expected
            Assert.NotNull(createdGame);
            Assert.Equal(whitePlayerId, createdGame.WhitePlayerId);
            Assert.Equal(blackPlayerId, createdGame.BlackPlayerId);
        
            // Verify the game exists in the database
            var retrievedGame = await context.Games.FirstOrDefaultAsync(g => g.Id == createdGame.Id);
            Assert.NotNull(retrievedGame);
            Assert.Equal(whitePlayerId, retrievedGame.WhitePlayerId);
            Assert.Equal(blackPlayerId, retrievedGame.BlackPlayerId);
        }
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