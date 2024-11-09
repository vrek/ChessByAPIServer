using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using ChessByAPIServer.Models.APIModels;
using ChessByAPIServer.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController(
    IGameRepository gameRepository,
    IChessBoardRepository chessBoardRepository,
    IUserRepository userRepository,
    IMoveRepository moveRepository)
    : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IChessBoardRepository _chessBoardRepository = chessBoardRepository;
    private readonly IMoveRepository _moveRepository = moveRepository;
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
    public async Task<Game> CreateGame(int whitePlayerId, int blackPlayerId)
    {
        try
        {
            // Use the repository to create a new game
            Game? createdGame = await _gameRepository.CreateGameAsync(whitePlayerId, blackPlayerId);

            // Return the created game with a 201 Created response
            if (createdGame != null)
            {
                return createdGame;
            }
            else
            {
                throw new Exception("Game could not be created");
            }

        }
        catch
        {
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostMove([FromBody] SubmittedMove move)
    {
        if (move == null)
            return Results.Problem(
                type: "Bad Request",
                title: "Move not Specified",
                detail: "The body of the move request can not be empty",
                statusCode: StatusCodes.Status400BadRequest) as IActionResult;
        var gameId = move.gameId;
        var player = move.fromPlayerId;
        var subMove = move.move;
        var subTime = move.dateSubmitted;

        var (isValid, errorMessage) = await _gameRepository.ValidateMoveAsync(gameId, player);
        if (!isValid)
            return Results.Problem(
                    type: "Bad Request",
                    title: "Validation Error",
                    detail: errorMessage,
                    statusCode: StatusCodes.Status400BadRequest)
                as IActionResult;


        return CreatedAtAction(nameof(GetGame), new { id = gameId }, subMove);
    }
}