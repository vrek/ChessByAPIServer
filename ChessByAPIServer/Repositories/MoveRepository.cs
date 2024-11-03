using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;

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

    public async Task<bool> IsValidMove(string piece, string start, string end, string color = null)
    {
        int StartRow, EndRow;

        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            throw new ArgumentNullException("Start and end positions must not be null or empty.");
        if (string.IsNullOrEmpty(piece))
            throw new ArgumentException("Piece type must not be null or empty.", nameof(piece));
        if (start == null || end == null) return false;

        var StartColumn = start[0].ToString().ToLower();
        var EndColumn = end[0].ToString().ToLower();
        if (!int.TryParse(start[1].ToString(), out StartRow) || !int.TryParse(end[1].ToString(), out EndRow))
            return false; // Return false if parsing rows failed
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

    public async Task<bool> IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow, string color = null)
{
    if (string.IsNullOrEmpty(color)) return false;
    color = color.ToLower();

    var isSameColumn = StartColumn == EndColumn;
    var isOneColumnAway = Math.Abs(StartColumn[0] - EndColumn[0]) == 1;

    if (color == "white")
    {
        var isOneRowForward = EndRow == StartRow + 1;

        // Check if destination square is occupied for capture or regular move
        bool isOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{EndColumn}{EndRow}");

        if (isOccupied)
        {
            // Capture only allowed diagonally (one row forward and one column to the left or right)
            if (isOneColumnAway && isOneRowForward)
            {
                return await IsPieceOpponents(EndColumn, EndRow, color);
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
    else if (color == "black")
    {
        var isOneRowBackward = EndRow == StartRow - 1;

        // Check if destination square is occupied for capture or regular move
        bool isOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{EndColumn}{EndRow}");

        if (isOccupied)
        {
            // Capture only allowed diagonally (one row backward and one column to the left or right)
            if (isOneColumnAway && isOneRowBackward)
            {
                return await IsPieceOpponents(EndColumn, EndRow, color);
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

    private async Task<bool> IsPieceOpponents(string EndColumn, int EndRow, string color)
    {
        string? pieceColor = await _board.GetPieceColorAtPositionAsync(_gameRepository.GetChessDbContext(), _game.Id,
            $"{EndColumn}{EndRow}");
        if (pieceColor == null)
        {
            throw new ArgumentException("Invalid piece position", nameof(pieceColor));
        }
        if (pieceColor != color) return true;
        return false;
    }


    public bool IsKnightMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
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

    public async Task SetSquareOccupiedAsync(string square, string pieceType, string color)
    {
        await _board.UpdatePositionAsync(_gameRepository.GetChessDbContext(), _game.Id, square, pieceType, color);
    }
}