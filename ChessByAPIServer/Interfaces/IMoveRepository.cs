using ChessByAPIServer.Enum;

namespace ChessByAPIServer.Repositories;

public interface IMoveRepository
{
    Task<bool> IsValidMove(string piece, string start, string end, PlayerRole? color = null);
    Task<bool> IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow, PlayerRole color);
    bool IsKnightMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsBishopMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsRookMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsQueenMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsKingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsCastlingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    Task<bool> IsEnPassantMoveValidAsync(int startRow, string endColumn, int endRow);
    bool IsInCheck(string position);
    bool IsInCheckMate(string position);
    bool IsInStalemate();
}