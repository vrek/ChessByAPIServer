using ChessByAPIServer.Enum;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer.Repositories;

public class MoveValidationRepository : IMoveValidationRepository
{
    private readonly GameRepository _gameRepository;
    private readonly IChessBoardRepository _board;
    private readonly Game _game;

    public MoveValidationRepository(Game game, IChessBoardRepository board, GameRepository gameRepository)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<bool> IsValidMove(string piece, string start, string end, PlayerRole? playerColor = null)
    {
        int StartRow, EndRow;
        PlayerRole color;
        if (string.IsNullOrEmpty(piece))
            throw new ArgumentException("Piece type must not be null or empty.", nameof(piece));
        if (playerColor != null)
        {
            color = (PlayerRole)playerColor!;
        }
        else
        {
            color = PlayerRole.All;
        }

        var StartColumn = start[0].ToString().ToLower();
        var EndColumn = end[0].ToString().ToLower();
        if (!int.TryParse(start[1].ToString(), out StartRow) || !int.TryParse(end[1].ToString(), out EndRow))
            return false; // Return false if parsing rows failed
        if (!IsPositionValid(StartColumn, StartRow) || !IsPositionValid(EndColumn, EndRow)) return false ;
        return piece.ToLower() switch
        {
            "pawn" => await IsPawnMoveValid(StartColumn, StartRow, EndColumn, EndRow, color),
            "knight" => IsKnightMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "bishop" => IsBishopMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "rook" => IsRookMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "queen" => IsQueenMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "king" => IsKingMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            _ => throw new ArgumentException("Invalid piece type", nameof(piece))
        };
    }

    public bool IsPositionValid(string column, int row)
    {
        // Validate column
        if (string.IsNullOrEmpty(column) || column.Length != 1)
            return false;

        // Validate column (must be between 'a' and 'h')
        char colChar = column[0];
        if (colChar < 'a' || colChar > 'h')
            return false;

        // Validate row (must be a digit between 1 and 8)
        if ( row < 1 || row > 8)
            return false;

        return true; // All checks passed
    }
    public async Task<bool> IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow, PlayerRole playerColor)
{
    if (string.IsNullOrEmpty(playerColor.ToString())) return false;
    

    var isSameColumn = StartColumn == EndColumn;
    var isOneColumnAway = Math.Abs(StartColumn[0] - EndColumn[0]) == 1;

    if (playerColor == PlayerRole.White)
    {
        var isOneRowForward = EndRow == StartRow + 1;

        // Check if destination square is occupied for capture or regular move
        bool isOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{EndColumn}{EndRow}");

        if (isOccupied)
        {
            // Capture only allowed diagonally (one row forward and one column to the left or right)
            if (isOneColumnAway && isOneRowForward)
            {
                return await IsPieceOpponents(EndColumn, EndRow, playerColor);
            }
        }
        else
        {
            // Regular forward moves
            if (isSameColumn && isOneRowForward)
                return true;

            // Double-step forward from initial row if both squares are unoccupied
            if (StartRow == 2 && isSameColumn && EndRow == 4)
            {
                var intermediateSquareOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{StartColumn}{StartRow + 1}");
                if (!intermediateSquareOccupied)
                    return true;
            }
        }
    }
    else if (playerColor == PlayerRole.Black)
    {
        var isOneRowBackward = EndRow == StartRow - 1;

        // Check if destination square is occupied for capture or regular move
        bool isOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{EndColumn}{EndRow}");

        if (isOccupied)
        {
            // Capture only allowed diagonally (one row backward and one column to the left or right)
            if (isOneColumnAway && isOneRowBackward)
            {
                return await IsPieceOpponents(EndColumn, EndRow, playerColor);
            }
        }
        else
        {
            // Regular backward moves
            if (isSameColumn && isOneRowBackward)
                return true;

            // Double-step backward from initial row if both squares are unoccupied
            if (StartRow == 7 && isSameColumn && EndRow == 5)
            {
                var intermediateSquareOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{StartColumn}{StartRow - 1}");
                if (!intermediateSquareOccupied)
                    return true;
            }
        }
    }

    return false;
}

    public async Task<bool> IsPieceOpponents(string EndColumn, int EndRow, PlayerRole playerColor)
    {
        PlayerRole? pieceColor = await _board.GetPieceColorAtPositionAsync(_gameRepository.GetChessDbContext(), _game.Id,
            $"{EndColumn}{EndRow}");
        if (pieceColor == null)
        {
            throw new ArgumentException("Invalid piece color", nameof(pieceColor));
        }
        if (pieceColor != playerColor) return true;
        return false;
    }


    public bool IsKnightMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        // Check for valid start and end positions
        if (string.IsNullOrEmpty(startColumn) || string.IsNullOrEmpty(endColumn))
            return false;

        // Convert columns from letters to numbers (a=1, b=2, ..., h=8)
        int startColNum = startColumn[0] - 'a' + 1; 
        int endColNum = endColumn[0] - 'a' + 1;

        // Calculate the differences in column and row
        int colDiff = Math.Abs(endColNum - startColNum);
        int rowDiff = Math.Abs(endRow - startRow);

        // A knight moves in an "L" shape: (2, 1) or (1, 2)
        return (colDiff == 2 && rowDiff == 1) || (colDiff == 1 && rowDiff == 2);
    }

    public bool IsBishopMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
    }

    public bool IsRookMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
    }

    public bool IsQueenMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
    }

    public bool IsKingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
    }

    public bool IsCastlingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsEnPassantMoveValidAsync(int startRow, string endColumn, int endRow)
    {
        var context = _gameRepository.GetChessDbContext();

        var endColChar = char.ToLower(endColumn[0]);

        // Check if the target square to the left or right of the pawn is occupied
        var leftPosition = $"{(char)(endColChar - 1)}{startRow}";
        var rightPosition = $"{(char)(endColChar + 1)}{startRow}";

        var leftPiece = await _board.GetPieceAtPositionAsync(context, _game.Id, leftPosition);
        var rightPiece = await _board.GetPieceAtPositionAsync(context, _game.Id, rightPosition);

        if (leftPiece == null && rightPiece == null) return false;

        // If either adjacent square has a piece, it could be a valid en passant move
        return true;
    }

    public bool IsInCheck(string position)
    {
        throw new NotImplementedException();
    }

    public bool IsInCheckMate(string position)
    {
        throw new NotImplementedException();
    }

    public bool IsInStalemate()
    {
        throw new NotImplementedException();
    }

    public async Task SetSquareOccupiedAsync(string square, string pieceType, PlayerRole color)
    {
        await _board.UpdatePositionAsync(_gameRepository.GetChessDbContext(), _game.Id, square, pieceType, color);
    }

    public async Task AddMoveToDbAsync(string startPosition, string endPosition, string color)
    {
        var context = _gameRepository.GetChessDbContext();
        var gameMove = new GameMove();
        string piece = await _board.GetPieceAtPositionAsync(context, _game.Id, startPosition);
        gameMove.GameGuid = _game.Id;
        gameMove.MoveNumber = await GetMostRecentMoveNumberAsync(context, _game.Id) + 1;
        gameMove.MoveNotation = await GetLongAlgebraicNotationAsync(context, piece,startPosition, endPosition, color);
        context.GameMoves.Add(gameMove);
        await context.SaveChangesAsync();
    }
    public async Task<string> GetLongAlgebraicNotationAsync(
        ChessDbContext context,
        string piece, 
        string startPosition, 
        string endPosition, 
        string color
      )
    {
        
        // Check if there is an opponent's piece on the end position
        var targetPosition = await context.ChessPositions
            .FirstOrDefaultAsync(p => p.GameId == _game.Id && p.Position == endPosition);

        bool isCapture = targetPosition != null && targetPosition.PieceColor != color;

        string moveNotation;
        // If piece is a Knight, use "N"; otherwise, use the first letter of the piece
        if (piece.Equals("Knight", StringComparison.OrdinalIgnoreCase))
        {
            moveNotation = "N";
        }
        else
        {
            moveNotation = piece.Substring(0, 1).ToUpper();
        }
        
        if (isCapture)
        {
            moveNotation += startPosition + "x" + endPosition;
        }
        else
        {
            moveNotation += startPosition + "-" + endPosition;
        }

        return moveNotation;
    }
    public async Task<int> GetMostRecentMoveNumberAsync(ChessDbContext context, Guid gameGuid)
    {
        var mostRecentMoveNumber = await context.GameMoves
            .Where(move => move.GameGuid == gameGuid)
            .OrderByDescending(move => move.MoveNumber)
            .Select(move => move.MoveNumber)
            .FirstOrDefaultAsync();

        return mostRecentMoveNumber;
    }

    public bool IsEndMoveDiagonal(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        // Validate input lengths first
        if (StartColumn.Length != 1 || EndColumn.Length != 1) return false;

        // Convert columns ('a'-'h') to numbers (1-8)
        int startColNum = StartColumn[0] - 'a' + 1;
        int endColNum = EndColumn[0] - 'a' + 1;
        
        // Check if the move is diagonal
        return Math.Abs(startColNum - endColNum) == Math.Abs(StartRow - EndRow);
    }

}