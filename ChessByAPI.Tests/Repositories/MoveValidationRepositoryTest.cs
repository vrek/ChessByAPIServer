using ChessByAPIServer;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChessByAPI.Tests.Repositories;

public class MoveValidationRepositoryTest : IDisposable
{
    private ChessDbContext _context;
    private MoveValidationRepository _moveValidator;
    private ChessBoardRepository boardRepo;
    private Game game;
    private GameRepository gameRepo;

    public MoveValidationRepositoryTest()
    {
        InitializeTest();
    }

    // Method to set up a new game and reinitialize the board for each test
    private void InitializeTest()
    {
        _context = GetInMemoryDbContext();
        game = new Game(Guid.NewGuid(), 1, 2);
        _context.Games.Add(game);
        _context.SaveChanges();
        
        boardRepo = new ChessBoardRepository();
        boardRepo.InitializeChessBoard(_context, game.Id);

        gameRepo = new GameRepository(_context);
        _moveValidator = new MoveValidationRepository(game, boardRepo, gameRepo);
    }

    private ChessDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ChessDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Theory]
    [InlineData("pawn", "e2", "e3", "white", true)]
    [InlineData("pawn", "e2", "e4", "white", true)]
    [InlineData("pawn", "e7", "e6", "black", true)]
    [InlineData("pawn", "e7", "e5", "black", true)]
    [InlineData("pawn", "e3", "e5", "white", false)]
    [InlineData("pawn", "e2", "d3", "white", false)]
    [InlineData("pawn", "d4", "e5", "white", false)]
    [InlineData("pawn", "d7", "d4", "white", false)]
    public async Task IsValidMove_ShouldReturnExpectedResult(string piece, string start, string end, string color, bool expected)
    {
        InitializeTest(); // Reinitialize board state before each test

        var result = await _moveValidator.IsValidMove(piece, start, end, color);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("e2", "e3", true, false)]  // Square in front occupied, expect false
    [InlineData("e2", "e3", false, true)]  // Square in front not occupied, expect true
    public async Task Pawn_CannotMoveForward_IfSquareIsOccupied(string start, string end, bool isEndOccupied, bool expectedResult)
    {
        InitializeTest(); // Reinitialize board state before each test

        // Arrange: Set the end square's occupied status
        if (isEndOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn", "white"); // Occupy the end square
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null, "white");   // Ensure the end square is unoccupied
        }

        // Act: Check if the pawn move is valid
        var result = await _moveValidator.IsValidMove("pawn", start, end, "white");

        // Assert: Verify the result matches expectedResult
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("e2", "f3", true,"white","white", false)]   // Diagonal square occupied with own piece as white, expect true (valid capture)
    [InlineData("e2", "f3", true,"black","black", false)]   // Diagonal square occupied with own piece as black, expect true (valid capture)
    [InlineData("e2", "f3", true,"white","black", true)]   // Diagonal square occupied with opponent piece, expect true (valid capture)
    [InlineData("e2", "f3", false, "white","black",false)] // Diagonal square not occupied, expect false
    public async Task Pawn_CanMoveDiagonally_OnlyIfSquareIsOccupied(string start, string end, bool isEndOccupied,string playersColor, string capturePieceColor, bool expectedResult)
    {
        InitializeTest(); // Reinitialize board state before each test

        // Arrange: Set the end square's occupied status
        if (isEndOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn", capturePieceColor); // Occupy the end square
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null,capturePieceColor);   // Ensure the end square is unoccupied
        }

        // Act: Check if the pawn move is valid
        var result = await _moveValidator.IsValidMove("pawn", start, end, playersColor);

        // Assert: Verify the result matches expectedResult
        Assert.Equal(expectedResult, result);
    }


    [Theory]
    [InlineData("e2", "e4", "e3", false, false, true)]
    [InlineData("e2", "e4", "e3", true, false, false)]
    [InlineData("e2", "e4", "e3", false, true, false)]
    [InlineData("e2", "e4", "e3", true, true, false)]
    public async Task Pawn_CanMoveTwoSquaresFromStart_OnlyIfBothSquaresUnoccupied(string start, string end, string intermediateSquare, bool isIntermediateOccupied, bool isDestinationOccupied, bool expectedResult)
    {
        InitializeTest(); // Reinitialize board state before each test

        if (isIntermediateOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(intermediateSquare, "pawn","white");
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(intermediateSquare, null,"white");
        }

        if (isDestinationOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn","white");
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null,"white");
        }

        var result = await _moveValidator.IsValidMove("pawn", start, end, "white");

        Assert.Equal(expectedResult, result);
    }
}
