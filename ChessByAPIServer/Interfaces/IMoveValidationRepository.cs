namespace ChessByAPIServer.Repositories;

public interface IMoveValidationRepository
{
    Task<bool> IsValidMove(string piece, string start, string end, string color = null);
    Task<bool> IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow, string color = null);
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