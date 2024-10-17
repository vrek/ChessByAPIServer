using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer.Repositories;

public class GameRepository(ChessDbContext context) : IGameRepository
{
    private readonly ChessDbContext _context = context;

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .Include(g => g.ChessPositions)
            .Include(g => g.GameMoves)
            .ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        Game? game = await _context.Games
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .Include(g => g.ChessPositions)
            .Include(g => g.GameMoves)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
        {
            return null;
        }

        return game;
    }

    public async Task<Game> AddAsync(Game game)
    {
        game.StartTime = DateTime.Now;
        _ = _context.Games.Add(game);
        _ = await _context.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Entry(game).State = EntityState.Modified;
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        Game? game = await _context.Games.FindAsync(id);
        if (game != null)
        {
            _ = _context.Games.Remove(game);
            _ = await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Games.AnyAsync(g => g.Id == id);
    }
}
