using ChessByAPIServer.Models;
using ChessByAPIServer.Enum;
namespace ChessByAPIServer.Interfaces;


public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);
    Task<Game> CreateGameAsync(int whitePlayerId, int blackPlayerId);
    Task<List<Game>> GetGamesByPlayerIdAsync(int playerId, PlayerRole role = PlayerRole.All);
}