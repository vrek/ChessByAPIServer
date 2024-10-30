using ChessByAPIServer;
using ChessByAPIServer.Controllers;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.EntityFrameworkCore;
using ChessByAPIServer.Enum;
using Xunit;

namespace ChessByAPI.Tests.Repositories
{
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
            int playerId = 1;
            // Arrange
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
            {
                UserName = "testuser2",
                Email = "email2@test.com",
                Password = "password1234"
            };User user3 = new()
            {
                UserName = "testuser3",
                Email = "email3@test.com",
                Password = "password123"
            };
            User user4 = new()
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
            int playerId = 1;
            // Arrange
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
            {
                UserName = "testuser2",
                Email = "email2@test.com",
                Password = "password1234"
            };User user3 = new()
            {
                UserName = "testuser3",
                Email = "email3@test.com",
                Password = "password123"
            };
            User user4 = new()
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
            int playerId = 1;
            // Arrange
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
            {
                UserName = "testuser2",
                Email = "email2@test.com",
                Password = "password1234"
            };User user3 = new()
            {
                UserName = "testuser3",
                Email = "email3@test.com",
                Password = "password123"
            };
            User user4 = new()
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
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
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
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
            {
                UserName = "testuser2",
                Email = "email2@test.com",
                Password = "password1234"
            };
            _ = await _context.Users.AddAsync(user1);
            _ = await _context.SaveChangesAsync();
            _ = await _context.Users.AddAsync(user2);
            _ = await _context.SaveChangesAsync();
            Guid GameId = Guid.NewGuid();
            Game newGame = new Game(GameId, user1.Id, user2.Id); 
            GameRepository gameRepository = new(_context);
            await gameRepository.CreateGameAsync(user1.Id, user2.Id);
            
            //string? piece = await ChessBoardRepository.GetPieceAtPositionAsync(_context, GameId,"a1");
            
            Assert.NotNull(newGame.ChessPositions);
        }
        [Fact]
        public async Task NewGameShouldHave64Positions()
        {
            // Arrange: Seed game with starting positions
            User user1 = new()
            {
                UserName = "testuser",
                Email = "email1@test.com",
                Password = "password123"
            };
            User user2 = new()
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
            int positionsCount = gameWithPositions?.ChessPositions.Count ?? 0;
            Assert.Equal(64, positionsCount);
        }
    }
    
    

    // [Fact]
    // public async Task GetGamesAsyncByPlayerID_ShouldReturnGamesWherePlayerIsBlack()
    // {
    //     // Arrange: Set up game data where player ID 2 is a black player
    //     var playerId = 2;
    //     Game game1 = new() { WhitePlayerId = 1, BlackPlayerId = playerId };
    //     Game game2 = new() { WhitePlayerId = 3, BlackPlayerId = playerId };
    //     Game game3 = new() { WhitePlayerId = playerId, BlackPlayerId = 4 }; // player as White
    //     await _context.Games.AddRangeAsync(game1, game2, game3);
    //     await _context.SaveChangesAsync();
    //
    //     // Act: Retrieve games where player ID is the black player
    //     var result = await _gameRepository.GetGamesAsyncByPlayerID(playerId);
    //
    //     // Assert: Only games where player is BlackPlayerId should be returned
    //     Assert.Contains(result, g => g.BlackPlayerId == playerId);
    //     Assert.DoesNotContain(result, g => g.WhitePlayerId == playerId && g.BlackPlayerId != playerId);
    // }
    //
    // [Fact]
    // public async Task GetGamesAsyncByPlayerID_ShouldReturnGamesWherePlayerIsEitherColor()
    // {
    //     // Arrange: Set up games where player 1 is both white and black in different games
    //     var playerId = 1;
    //     Game game1 = new() { WhitePlayerId = playerId, BlackPlayerId = 2 };
    //     Game game2 = new() { WhitePlayerId = 3, BlackPlayerId = playerId };
    //     await _context.Games.AddRangeAsync(game1, game2);
    //     await _context.SaveChangesAsync();
    //
    //     // Act: Retrieve games where player ID is either white or black
    //     var result = await _gameRepository.GetGamesAsyncByPlayerID(playerId);
    //
    //     // Assert: Both games where player is either WhitePlayerId or BlackPlayerId should be returned
    //     Assert.Equal(2, result.Count);
    //     Assert.Contains(result, g => g.WhitePlayerId == playerId);
    //     Assert.Contains(result, g => g.BlackPlayerId == playerId);
    // }
    //
    // [Fact]
    // public async Task GetGamesAsyncByPlayerID_ShouldReturnEmptyForNonExistentPlayer()
    // {
    //     // Arrange: Add sample games without involving player ID 999
    //     Game game1 = new() { WhitePlayerId = 1, BlackPlayerId = 2 };
    //     Game game2 = new() { WhitePlayerId = 3, BlackPlayerId = 4 };
    //     await _context.Games.AddRangeAsync(game1, game2);
    //     await _context.SaveChangesAsync();
    //
    //     // Act: Try to retrieve games for a non-existent player ID
    //     var invalidPlayerId = 999;
    //     var result = await _gameRepository.GetGamesAsyncByPlayerID(invalidPlayerId);
    //
    //     // Assert: Result should be empty for a non-existent player ID
    //     Assert.Empty(result);
    // }
    //
    // [Fact]
    // public async Task GetGamesAsyncByPlayerID_ShouldReturnEmptyForDeletedPlayer()
    // {
    //     // Arrange: Add a deleted player and associated games
    //     var playerId = 5;
    //     Game game1 = new() { WhitePlayerId = playerId, BlackPlayerId = 6 };
    //     Game game2 = new() { WhitePlayerId = 7, BlackPlayerId = playerId };
    //     User deletedPlayer = new() { Id = playerId, IsDeleted = true };
    //     await _context.Games.AddRangeAsync(game1, game2);
    //     await _context.Users.AddAsync(deletedPlayer);
    //     await _context.SaveChangesAsync();
    //
    //     // Act: Retrieve games where player ID is a deleted player
    //     var result = await _gameRepository.GetGamesAsyncByPlayerID(playerId);
    //
    //     // Assert: Result should be empty since the player is marked as deleted
    //     Assert.Empty(result);
    // }
}