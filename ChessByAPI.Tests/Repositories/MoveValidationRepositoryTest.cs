using ChessByAPIServer;
using ChessByAPIServer.Enum;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Xunit;

public class HelperMoveValidationRepositoryTest : IDisposable
{
    private ChessDbContext _context;
    private MoveValidationRepository _Validator;
    private ChessBoardRepository boardRepo;
    private Game game;
    private GameRepository gameRepo;

    public HelperMoveValidationRepositoryTest()
    {
        InitializeTest();
    }

    private void InitializeTest()
    {
        _context = GetInMemoryDbContext();
        game = new Game(Guid.NewGuid(), 1, 2);
        _context.Games.Add(game);
        _context.SaveChanges();

        boardRepo = new ChessBoardRepository();
        boardRepo.InitializeChessBoard(_context, game.Id);

        gameRepo = new GameRepository(_context);
        _Validator = new MoveValidationRepository(game, boardRepo, gameRepo);
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
    [InlineData("a", 1, true)]  // Valid position
    [InlineData("h", 8, true)]  // Valid position
    [InlineData("d", 4, true)]  // Valid position
    [InlineData("b", 7, true)]  // Valid position
    public void IsPositionValid_ValidInputs_ShouldReturnTrue(string column, int row, bool expected)
    {
        // Act
        var result = _Validator.IsPositionValid(column, row);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", 1, false)]   // Empty column
    [InlineData("aa", 1, false)]  // Invalid column length
    [InlineData("z", 1, false)]   // Column out of range
    [InlineData("h", 0, false)]   // Row below valid range
    [InlineData("h", 9, false)]   // Row above valid range
    [InlineData("c", -1, false)]  // Negative row
    public void IsPositionValid_InvalidInputs_ShouldReturnFalse(string column, int row, bool expected)
    {
        // Act
        var result = _Validator.IsPositionValid(column, row);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("a", 0, false)]  // Row is zero (invalid)
    [InlineData("a", 9, false)]  // Row exceeds valid range
    [InlineData("a", 5, true)]   // Valid position with a valid row
    [InlineData("b", 1, true)]   // Valid position with a valid row
    [InlineData("e", 8, true)]   // Valid position with a valid row
    public void IsPositionValid_AdditionalCases_ShouldReturnExpected(string column, int row, bool expected)
    {
        // Act
        var result = _Validator.IsPositionValid(column, row);

        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("a", 1, PlayerRole.White, false)]
    [InlineData("c", 2, PlayerRole.White, false)]
    [InlineData("c", 8, PlayerRole.White, true)]
    [InlineData("h", 8, PlayerRole.White, true)]
    [InlineData("a", 1, PlayerRole.Black, true)]
    [InlineData("c", 2, PlayerRole.Black, true)]
    [InlineData("c", 8, PlayerRole.Black, false)]
    [InlineData("h", 8, PlayerRole.Black, false)]
    public void IsOpponentsPieceShouldReturnExpected(string column, int row, PlayerRole playerColor, bool expected)
    {
        //Act
        var result = _Validator.IsPieceOpponents(column, row, playerColor);
    }
    [Theory]
    [InlineData("Knight", "g1", "f3", "White", false, "Ng1-f3")]
    [InlineData("Knight", "g1", "f3", "White", true, "Ng1xf3")]
    [InlineData("Pawn", "e4", "d5", "White", true, "Pe4xd5")]
    [InlineData("Pawn", "e2", "e4", "White", false, "Pe2-e4")]
    [InlineData("Dragon", "e2", "e4", "White", false, "De2-e4")]
    public async Task GetLongAlgebraicNotationAsync_ShouldReturnCorrectNotation(
        string piece, 
        string startPosition, 
        string endPosition, 
        string color, 
        bool isCapture,
        string expectedNotation)
    {
        // Arrange: Set up the target position in the database for capture or non-capture cases
        if (isCapture)
        {
            await _context.ChessPositions.AddAsync(new ChessPosition
            {
                GameId = game.Id, // Assume GameId property exists or set manually
                Position = endPosition,
                PieceColor = color == "White" ? "Black" : "White" // Opponent's color for capture
            });
        }
        else
        {
            // Ensure no piece is present at the end position for non-capture cases
            var position = await _context.ChessPositions
                .FirstOrDefaultAsync(p => p.GameId == game.Id && p.Position == endPosition);
            if (position != null)
            {
                _context.ChessPositions.Remove(position);
            }
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _Validator.GetLongAlgebraicNotationAsync(_context, piece, startPosition, endPosition, color);

        // Assert
        Assert.Equal(expectedNotation, result);
    }
    [Theory]
    [InlineData("a",1, "g",7, true)]  // Valid position
    [InlineData("b",4, "f",8, true)]  // Valid position
    [InlineData("h",7, "e",4, true)]  // Valid position
    [InlineData("g",3, "d",6, true)]  // Valid position
    [InlineData("a",1, "a",7, false)]  // Valid position
    [InlineData("b",4, "g",6, false)]  // Valid position
    [InlineData("h",7, "f",3, false)]  // Valid position
    [InlineData("g",3, "g",3, true)]  // Valid position
    public void IsEndMoveDiagonalShouldReturnCorrectAnswer(string StartColumn, int StartRow, string EndColumn, int EndRow, bool expected)
    {
        // Act
        var result = _Validator.IsEndMoveDiagonal(StartColumn, StartRow,EndColumn, EndRow);
        
        // Assert
        Assert.Equal(expected, result);
    }
}


public class PawnMoveValidationRepositoryTest : IDisposable
{
    private ChessDbContext _context;
    private MoveValidationRepository _moveValidator;
    private ChessBoardRepository boardRepo;
    private Game game;
    private GameRepository gameRepo;

    public PawnMoveValidationRepositoryTest()
    {
        InitializeTest();
    }

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
    [InlineData("pawn", "e2", "e3", PlayerRole.White, true)]
    [InlineData("pawn", "e2", "e4", PlayerRole.White, true)]
    [InlineData("pawn", "e7", "e6", PlayerRole.Black, true)]
    [InlineData("pawn", "e7", "e5", PlayerRole.Black, true)]
    [InlineData("pawn", "e3", "e5", PlayerRole.White, false)]
    [InlineData("pawn", "e2", "d3", PlayerRole.White, false)]
    [InlineData("pawn", "d4", "e5", PlayerRole.White, false)]
    [InlineData("pawn", "d7", "d4", PlayerRole.White, false)]
    public async Task IsValidMove_ShouldReturnExpectedResult(string piece, string start, string end, PlayerRole color, bool expected)
    {
        InitializeTest(); // Reinitialize board state before each test

        var result = await _moveValidator.IsValidMove(piece, start, end, color);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("e2", "e3", true, false)]
    [InlineData("e2", "e3", false, true)]
    public async Task Pawn_CannotMoveForward_IfSquareIsOccupied(string start, string end, bool isEndOccupied, bool expectedResult)
    {
        InitializeTest(); // Reinitialize board state before each test

        if (isEndOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn", PlayerRole.White);
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null, PlayerRole.White);
        }

        var result = await _moveValidator.IsValidMove("pawn", start, end, PlayerRole.White);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("e2", "f3", true, PlayerRole.White, PlayerRole.White, false)]
    [InlineData("e2", "f3", true, PlayerRole.Black, PlayerRole.Black, false)]
    [InlineData("e2", "f3", true, PlayerRole.White, PlayerRole.Black, true)]
    [InlineData("e2", "f3", false, PlayerRole.White, PlayerRole.Black, false)]
    public async Task Pawn_CanMoveDiagonally_OnlyIfSquareIsOccupied(string start, string end, bool isEndOccupied, PlayerRole playersColor, PlayerRole capturePieceColor, bool expectedResult)
    {
        InitializeTest(); // Reinitialize board state before each test

        if (isEndOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn", capturePieceColor);
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null, capturePieceColor);
        }

        var result = await _moveValidator.IsValidMove("pawn", start, end, playersColor);

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
            await _moveValidator.SetSquareOccupiedAsync(intermediateSquare, "pawn", PlayerRole.White);
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(intermediateSquare, null, PlayerRole.White);
        }

        if (isDestinationOccupied)
        {
            await _moveValidator.SetSquareOccupiedAsync(end, "pawn", PlayerRole.White);
        }
        else
        {
            await _moveValidator.SetSquareOccupiedAsync(end, null, PlayerRole.White);
        }

        var result = await _moveValidator.IsValidMove("pawn", start, end, PlayerRole.White);

        Assert.Equal(expectedResult, result);
    }
}

public class KnightMoveValidationRepositoryTest : IDisposable
{
    private ChessDbContext _context;
    private MoveValidationRepository _moveValidator;
    private ChessBoardRepository boardRepo;
    private Game game;
    private GameRepository gameRepo;

    public KnightMoveValidationRepositoryTest()
    {
        InitializeTest();
    }

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
    [InlineData("knight", "c5", "a6", PlayerRole.White, true)]
    [InlineData("knight", "c5", "b7", PlayerRole.White, true)]
    [InlineData("knight", "c5", "d7", PlayerRole.Black, true)]
    [InlineData("knight", "c5", "e6", PlayerRole.Black, true)]
    [InlineData("knight", "c5", "a4", PlayerRole.White, true)]
    [InlineData("knight", "c5", "b3", PlayerRole.White, true)]
    [InlineData("knight", "c5", "e4", PlayerRole.Black, true)]
    [InlineData("knight", "c5", "d3", PlayerRole.Black, true)]
    [InlineData("knight", "e3", "e5", PlayerRole.White, false)]
    [InlineData("knight", "e2", "d3", PlayerRole.White, false)]
    [InlineData("knight", "d4", "e5", PlayerRole.White, false)]
    [InlineData("knight", "d7", "d4", PlayerRole.White, false)]
    public async Task Knight_IsValidMove_ShouldReturnExpectedResult(string piece, string start, string end, PlayerRole color, bool expected)
    {
        InitializeTest(); // Reinitialize board state before each test

        var result = await _moveValidator.IsValidMove(piece, start, end, color);

        Assert.Equal(expected, result);
    }
}


