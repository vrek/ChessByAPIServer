using ChessByAPIServer;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPI.Tests;

public class DbContextTests
{
    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ChessDbContext(options);
    }

    [Fact] // XUnit test attribute
    public void AddNewGame_ShouldStoreCorrectly()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        Game game = new(Guid.NewGuid(), 1, 2);

        // Act
        _ = context.Games.Add(game);
        _ = context.SaveChanges();

        // Assert
        var storedGame = context.Games.FirstOrDefault(g => g.Id == game.Id);
        Assert.NotNull(storedGame); // Game should be stored
        Assert.Equal(game.WhitePlayerId, storedGame.WhitePlayerId);
        Assert.Equal(game.BlackPlayerId, storedGame.BlackPlayerId);
        Assert.Equal(game.StartTime, storedGame.StartTime);
    }
}