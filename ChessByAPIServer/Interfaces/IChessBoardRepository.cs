namespace ChessByAPIServer.Interfaces;

public interface IChessBoardRepository
{
    public Task<bool> InitializeChessBoard(ChessDbContext context, Guid gameId);
    public Task<string?> GetPieceAtPositionAsync(ChessDbContext context, Guid gameId, string position);
    public Task<bool> MovePieceToPositionAsync(ChessDbContext context, Guid gameId, string piece, string position);
}