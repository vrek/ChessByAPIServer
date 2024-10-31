namespace ChessByAPIServer.Repositories;

public interface IMoveValidationRepository
{
    bool IsValidMove(string piece, string start, string end);
    bool IsPawnMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsKnightMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsBishopMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsRookMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsQueenMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsKingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsCastlingMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsEnPassantMoveValid(string StartColumn, int StartRow, string EndColumn, int EndRow);
    bool IsInCheck(string position);
    bool IsInCheckMate(string position);
    bool IsInStalemate();
}