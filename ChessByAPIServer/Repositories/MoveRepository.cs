using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;

namespace ChessByAPIServer.Repositories;

public class MoveValidationRepository : IMoveValidationRepository
{
    private readonly Game _game;
    private readonly IChessBoardRepository _board;

    public MoveValidationRepository(Game game, IChessBoardRepository board)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _board = board ?? throw new ArgumentNullException(nameof(board));
    }

    public bool IsValidMove(string piece, string start, string end)
    {
        int StartRow, EndRow;

        if (start == null || end == null) return false;
        var StartColumn = start[0].ToString().ToLower();
        var EndColumn = end[0].ToString().ToLower();
        if (!int.TryParse(start[1].ToString(), out StartRow) || !int.TryParse(end[1].ToString(), out EndRow))
            return false; // Return false if parsing rows failed
        return piece.ToLower() switch
        {
            "pawn" => IsPawnMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "knight" => IsKnightMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "bishop" => IsBishopMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "rook" => IsRookMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "queen" => IsQueenMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            "king" => IsKingMoveValid(StartColumn, StartRow, EndColumn, EndRow),
            _ => throw new ArgumentException("Invalid piece type", nameof(piece))
        };
    }

    public bool IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        if (StartRow is 2 or 7)
        {
            if (StartColumn != EndColumn) return false;
            if (StartRow + 2 < EndRow) return false;
        }
        else{
            if (StartColumn != EndColumn) return false;
            if (StartRow + 1 < EndRow) return false;}  
        return true;
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

    public bool IsEnPassantMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow)
    {
        throw new NotImplementedException();
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
}