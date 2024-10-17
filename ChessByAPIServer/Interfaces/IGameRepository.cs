using ChessByAPIServer.Models;

namespace ChessByAPIServer.Interfaces;


public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game> GetByIdAsync(Guid id);
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

