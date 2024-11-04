using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.EntityFrameworkCore;
using ChessByAPIServer.Enum;
using Xunit;

namespace ChessByAPI.Tests.Repositories;

public class GameRepositoryTest : IDisposable
{
    private readonly ChessDbContext _context;
    private readonly GameRepository _gameRepository;

    public GameRepositoryTest()
    {
        // Set up the in-memory database and repository
        _context = GetInMemoryDbContext();
        _gameRepository = new GameRepository(_context);
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ChessDbContext(options);
    }

    [Fact]
    public async Task GetGamesAsyncByPlayerId_ShouldReturnGamesWherePlayerIsPlaying()
    {
        var playerId = 1;
        // Arrange
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
        User? user3 = new()
        {
            UserName = "testuser3",
            Email = "email3@test.com",
            Password = "password123"
        };
        User? user4 = new()
        {
            UserName = "testuser4",
            Email = "email4@test.com",
            Password = "password1234"
        };
        _ = await _context.Users.AddAsync(user1);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user3);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user4);
        _ = await _context.SaveChangesAsync();
        await _gameRepository.CreateGameAsync(playerId, 2); // Player is white
        await _gameRepository.CreateGameAsync(playerId, 3); // Player is white
        await _gameRepository.CreateGameAsync(4, playerId); // Player is black
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameRepository.GetGamesByPlayerIdAsync(playerId);

        // Assert
        Assert.Contains(result, g => g.WhitePlayerId == playerId);
    }

    [Fact]
    public async Task GetGamesAsyncByPlayerId_ShouldReturnGamesWherePlayerIsPlayingWhiteWhileSecondArgumentIsWhite()
    {
        var playerId = 1;
        // Arrange
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
        User? user3 = new()
        {
            UserName = "testuser3",
            Email = "email3@test.com",
            Password = "password123"
        };
        User? user4 = new()
        {
            UserName = "testuser4",
            Email = "email4@test.com",
            Password = "password1234"
        };
        _ = await _context.Users.AddAsync(user1);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user3);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user4);
        _ = await _context.SaveChangesAsync();
        await _gameRepository.CreateGameAsync(playerId, 2); // Player is white
        await _gameRepository.CreateGameAsync(playerId, 3); // Player is white
        await _gameRepository.CreateGameAsync(4, playerId); // Player is black
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameRepository.GetGamesByPlayerIdAsync(playerId, PlayerRole.White);

        // Assert
        Assert.Contains(result, g => g.WhitePlayerId == playerId);
        Assert.DoesNotContain(result, g => g.BlackPlayerId == playerId);
    }

    [Fact]
    public async Task GetGamesAsyncByPlayerId_ShouldReturnGamesWherePlayerIsPlayingBlackWhileSecondArgumentIsBlack()
    {
        var playerId = 1;
        // Arrange
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
        User? user3 = new()
        {
            UserName = "testuser3",
            Email = "email3@test.com",
            Password = "password123"
        };
        User? user4 = new()
        {
            UserName = "testuser4",
            Email = "email4@test.com",
            Password = "password1234"
        };
        _ = await _context.Users.AddAsync(user1);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user2);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user3);
        _ = await _context.SaveChangesAsync();
        _ = await _context.Users.AddAsync(user4);
        _ = await _context.SaveChangesAsync();
        await _gameRepository.CreateGameAsync(playerId, 2); // Player is white
        await _gameRepository.CreateGameAsync(playerId, 3); // Player is white
        await _gameRepository.CreateGameAsync(4, playerId); // Player is black
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameRepository.GetGamesByPlayerIdAsync(playerId, PlayerRole.Black);

        // Assert
        Assert.Contains(result, g => g.BlackPlayerId == playerId);
        Assert.DoesNotContain(result, g => g.WhitePlayerId == playerId);
    }

    public void Dispose()
    {
        // Teardown: Dispose of the context after each test
        _context.Dispose();
    }

    [Fact]
    public async Task NewGameHasPiecesInRightPositions()
    {
        // Arrange: Seed game with starting positions
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
        await _context.Users.AddAsync(user1);
        await _context.SaveChangesAsync();
        await _context.Users.AddAsync(user2);
        await _context.SaveChangesAsync();

        // Act: Create a new game and retrieve the game from the database
        GameRepository gameRepository = new(_context);
        var createdGame = await gameRepository.CreateGameAsync(user1.Id, user2.Id);

        // Fetch the newly created game along with its chess positions
        var gameWithPositions = await _context.Games
            .Include(g => g.ChessPositions)
            .FirstOrDefaultAsync(g => g.Id == createdGame.Id);

        // Act: Retrieve positions and pieces for validation
        var positions = await gameRepository.GetChessPositionsDict(gameWithPositions);

        // Define the expected starting positions
        var expectedPositions = new Dictionary<string, string?>
        {
            // White pieces
            { "a1", "Rook" }, { "b1", "Knight" }, { "c1", "Bishop" }, { "d1", "Queen" },
            { "e1", "King" }, { "f1", "Bishop" }, { "g1", "Knight" }, { "h1", "Rook" },
            { "a2", "Pawn" }, { "b2", "Pawn" }, { "c2", "Pawn" }, { "d2", "Pawn" },
            { "e2", "Pawn" }, { "f2", "Pawn" }, { "g2", "Pawn" }, { "h2", "Pawn" },

            // Black pieces
            { "a8", "Rook" }, { "b8", "Knight" }, { "c8", "Bishop" }, { "d8", "Queen" },
            { "e8", "King" }, { "f8", "Bishop" }, { "g8", "Knight" }, { "h8", "Rook" },
            { "a7", "Pawn" }, { "b7", "Pawn" }, { "c7", "Pawn" }, { "d7", "Pawn" },
            { "e7", "Pawn" }, { "f7", "Pawn" }, { "g7", "Pawn" }, { "h7", "Pawn" }
        };

        // Assert: Verify each position and piece match expected starting positions
        foreach (var expected in expectedPositions)
        {
            Assert.True(positions.ContainsKey(expected.Key), $"Missing position: {expected.Key}");
            Assert.Equal(expected.Value, positions[expected.Key]);
        }
    }

    [Fact]
    public async Task NewGameDoesNotHaveEmptyBoard()
    {
        // Arrange: Seed game with starting positions
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
        var GameId = Guid.NewGuid();
        var newGame = new Game(GameId, user1.Id, user2.Id);
        GameRepository gameRepository = new(_context);
        await gameRepository.CreateGameAsync(user1.Id, user2.Id);

        //string? piece = await ChessBoardRepository.GetPieceAtPositionAsync(_context, GameId,"a1");

        Assert.NotNull(newGame.ChessPositions);
    }

    [Fact]
    public async Task NewGameShouldHave64Positions()
    {
        // Arrange: Seed game with starting positions
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
        await _context.Users.AddAsync(user1);
        await _context.SaveChangesAsync();
        await _context.Users.AddAsync(user2);
        await _context.SaveChangesAsync();

        // Act: Create a new game and retrieve the game from the database
        GameRepository gameRepository = new(_context);
        var createdGame = await gameRepository.CreateGameAsync(user1.Id, user2.Id);

        // Fetch the newly created game along with its chess positions
        var gameWithPositions = await _context.Games
            .Include(g => g.ChessPositions)
            .FirstOrDefaultAsync(g => g.Id == createdGame.Id);

        // Assert: Verify there are 64 positions in the game
        var positionsCount = gameWithPositions?.ChessPositions.Count ?? 0;
        Assert.Equal(64, positionsCount);
    }
        private async Task SeedDatabaseAsync(ChessDbContext context, Game game)
    {
        // Seed game
        await context.Games.AddAsync(game);
        await context.SaveChangesAsync();
    
        // Seed initial positions
        await context.ChessPositions.AddRangeAsync(new[]
        {
            new ChessPosition { GameId = game.Id, Position = "e2", Piece = "Pawn", PieceColor = "White", IsEmpty = false },
            new ChessPosition { GameId = game.Id, Position = "e4", IsEmpty = true },
            new ChessPosition { GameId = game.Id, Position = "f2", Piece = "Pawn", PieceColor = "White", IsEmpty = false },
            new ChessPosition { GameId = game.Id, Position = "f4", IsEmpty = true }
        });
        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task TakeMoveAsync_ValidMove_ReturnsTrue()
    {
        // Arrange
        var game = new Game (Guid.NewGuid(), 1, 2);
        
        await SeedDatabaseAsync(_context, game);
    
        // Act
        var result = await _gameRepository.TakeMoveAsync(game, "e2", "e4", PlayerRole.White);
    
        // Assert
        Assert.True(result);
        var startPosition = await _context.ChessPositions.FirstAsync(cp => cp.Position == "e2" && cp.GameId == game.Id);
        var endPosition = await _context.ChessPositions.FirstAsync(cp => cp.Position == "e4" && cp.GameId == game.Id);
    
        Assert.True(startPosition.IsEmpty);
        Assert.False(endPosition.IsEmpty);
        Assert.Equal("Pawn", endPosition.Piece);
        Assert.Equal("White", endPosition.PieceColor);
    }
    
    [Fact]
    public async Task TakeMoveAsync_MoveFromEmptySquare_ReturnsFalse()
    {
        // Arrange
        var game = new Game (Guid.NewGuid(), 1, 2);
        using var context = GetInMemoryDbContext();
        await SeedDatabaseAsync(context, game);
    
        // Act
        var result = await _gameRepository.TakeMoveAsync(game, "e3", "e4", PlayerRole.White); // "e3" is empty
    
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task TakeMoveAsync_InvalidMove_ReturnsFalse()
    {
        // Arrange
        var game = new Game (Guid.NewGuid(), 1, 2);
        using var context = GetInMemoryDbContext();
        await SeedDatabaseAsync(context, game);
    
        // Act
        var result = await _gameRepository.TakeMoveAsync(game, "e2", "e5", PlayerRole.White); // Invalid move for a pawn
    
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task TakeMoveAsync_MoveWithoutPieceColor_ReturnsFalse()
    {
        // Arrange
        var game = new Game (Guid.NewGuid(), 1, 2);
        using var context = GetInMemoryDbContext();
    
        // Add a position without a color
        await context.ChessPositions.AddAsync(new ChessPosition
        {
            GameId = game.Id,
            Position = "e2",
            Piece = "Pawn",
            IsEmpty = false,
            PieceColor = null // No color set
        });
        await context.SaveChangesAsync();
    
        // Act
        var result = await _gameRepository.TakeMoveAsync(game, "e2", "e4", PlayerRole.White);
    
        // Assert
        Assert.False(result);
    }
}
