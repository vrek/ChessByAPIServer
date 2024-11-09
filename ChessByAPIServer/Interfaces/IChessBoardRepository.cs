using ChessByAPIServer.Contexts;
using ChessByAPIServer.Enum;
using ChessByAPIServer.Models;

namespace ChessByAPIServer.Interfaces;

public interface IChessBoardRepository
{
    public Task<bool> InitializeChessBoard(ChessDbContext context, Guid gameId);
    public Task<string?> GetPieceAtPositionAsync(ChessDbContext context, Guid gameId, string position);

    public Task<bool> IsSquareOccupied(ChessDbContext context, Guid gameId, string position);

    public Task<bool> UpdatePositionAsync(ChessDbContext context, Guid gameId, string position, string? newPiece,
        PlayerRole? newPieceColor);

    public Task<List<ChessPosition>> GetAllPositionsAsync(ChessDbContext context, Guid gameId);
    public Task<PlayerRole?> GetPieceColorAtPositionAsync(ChessDbContext context, Guid gameId, string position);
}