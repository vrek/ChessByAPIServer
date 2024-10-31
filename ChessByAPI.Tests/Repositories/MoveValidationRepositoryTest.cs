using ChessByAPIServer;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;


namespace ChessByAPI.Tests.Repositories;

[TestSubject(typeof(MoveValidationRepository))]
public class MoveValidationRepositoryTest
{

    private readonly MoveValidationRepository _moveValidator;
    private readonly ChessDbContext _context;

    public MoveValidationRepositoryTest()
    {
        // Setup MoveValidator instance with test Game and ChessBoardRepository as needed
        _context = GetInMemoryDbContext();
        var game = new Game(Guid.NewGuid(), 1,2);
        var boardRepo = new ChessBoardRepository();
        _moveValidator = new MoveValidationRepository(game, boardRepo);
        
    }
    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ChessDbContext(options);
    }
    
    [Theory]
    [InlineData("pawn", "e2", "e3", true)]  // White pawn single step forward
    [InlineData("pawn", "e2", "e4", true)]  // White pawn double step forward
    [InlineData("pawn", "e7", "e6", true)]  // Black pawn single step forward
    [InlineData("pawn", "e7", "e5", true)]  // Black pawn double step forward
    [InlineData("pawn", "e3", "e5", false)] // Invalid double step from non-starting position
    [InlineData("pawn", "e2", "d3", false)] // Invalid diagonal move without capture
    [InlineData("pawn", "d4", "e5", false)] // Invalid capture without capture logic (no en passant)
    [InlineData("pawn", "d7", "d4", false)] // Invalid three square move for black
    public void IsValidMove_ShouldReturnExpectedResult(string piece, string start, string end, bool expected)
    {
        // Act
        var result = _moveValidator.IsValidMove(piece, start, end);

        // Assert
        Assert.Equal(expected, result);
    }
}