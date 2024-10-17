using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GameController(IGameRepository gameRepository) : ControllerBase
{
    private readonly IGameRepository _gameRepository = gameRepository;

    // GET: api/Game
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames()
    {
        IEnumerable<Game> games = await _gameRepository.GetAllAsync();
        return Ok(games);
    }

    // GET: api/Game/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(Guid id)
    {
        Game game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    // POST: api/Game
    [HttpPost]
    public async Task<ActionResult<Game>> CreateGame(Game game)
    {
        Game createdGame = await _gameRepository.AddAsync(game);
        return CreatedAtAction(nameof(GetGame), new { id = createdGame.Id }, createdGame);
    }

    // PUT: api/Game/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(Guid id, Game game)
    {
        if (id != game.Id)
        {
            return BadRequest();
        }

        if (!await _gameRepository.ExistsAsync(id))
        {
            return NotFound();
        }

        await _gameRepository.UpdateAsync(game);
        return NoContent();
    }

    // DELETE: api/Game/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(Guid id)
    {
        if (!await _gameRepository.ExistsAsync(id))
        {
            return NotFound();
        }

        await _gameRepository.DeleteAsync(id);
        return NoContent();
    }
}
