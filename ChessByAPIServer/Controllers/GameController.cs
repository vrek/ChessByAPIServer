using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController(IGameRepository gameRepository) : ControllerBase
{
    private readonly IGameRepository _gameRepository = gameRepository;

    // GET: api/Game/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(Guid id)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null) return NotFound();

        return Ok(game);
    }

    // POST: api/Game/{whitePlayerId}/{blackPlayerId}
    [HttpPost("{whitePlayerId}/{blackPlayerId}")]
    public async Task<ActionResult<Game>> CreateGame(int whitePlayerId, int blackPlayerId)
    {
        try
        {
            // Use the repository to create a new game
            var createdGame = await _gameRepository.CreateGameAsync(whitePlayerId, blackPlayerId);

            // Return the created game with a 201 Created response
            return CreatedAtAction(nameof(GetGame), new { id = createdGame.Id }, createdGame);
        }
        catch (ArgumentException ex)
        {
            // If a validation error occurs, return a 400 Bad Request with the error message
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Handle any invalid operation exceptions that may arise
            return Conflict(ex.Message); // 409 Conflict if there's an issue with existing game rules
        }
        catch (Exception ex)
        {
            // Handle any unexpected exceptions
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}