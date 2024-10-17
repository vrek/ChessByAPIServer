namespace ChessByAPIServer.Interfaces;

public interface IChessBoardRepository
{
    public void InitializeChessBoard(ChessDbContext context, Guid gameId);
}
