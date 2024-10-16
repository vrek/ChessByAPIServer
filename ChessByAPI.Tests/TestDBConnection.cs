using ChessByAPIServer;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPI.Tests
{
    public class DBContextTests
    {
        private ChessDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ChessDbContext> options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ChessDbContext(options);
        }

        [Fact]  // XUnit test attribute
        public void AddNewGame_ShouldStoreCorrectly()
        {
            // Arrange
            ChessDbContext context = GetInMemoryDbContext();
            Game game = new()
            {
                Id = Guid.NewGuid(),
                WhitePlayerId = 1,
                BlackPlayerId = 2,
                StartTime = DateTime.Now
            };

            // Act
            _ = context.Games.Add(game);
            _ = context.SaveChanges();

            // Assert
            Game? storedGame = context.Games.FirstOrDefault(g => g.Id == game.Id);
            Assert.NotNull(storedGame);  // Game should be stored
            Assert.Equal(game.WhitePlayerId, storedGame.WhitePlayerId);
            Assert.Equal(game.BlackPlayerId, storedGame.BlackPlayerId);
            Assert.Equal(game.StartTime, storedGame.StartTime);
        }
    }
}
