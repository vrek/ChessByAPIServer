using ChessByAPIServer.Enum;
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
        var _game = await _gameRepository.GetByIdAsync(id);

        if (_game == null) return NotFound();

        return Ok(_game);
    }

    public async Task<PlayerRole> GetPlayerColor(Guid gameId, int playerId)
    {
        var _result = await GetGame(gameId);

        if (_result is ActionResult<Game> actionResult && actionResult.Value is Game game)
        {
            if (playerId == game.WhitePlayerId)
            {
                return PlayerRole.White;
            }
            else if (playerId == game.BlackPlayerId)
            {
                return PlayerRole.Black;
            }
            else
            {
                throw new ArgumentException($"Player {playerId} is not a participant in the specified game", nameof(playerId));
            }
        }
        else
        {
            throw new InvalidOperationException("Game not found or an error occurred.");
        }
    }


    // POST: api/Game/{whitePlayerId}/{blackPlayerId}
    [HttpPost("{whitePlayerId}/{blackPlayerId}")]
    public async Task<Game> CreateGame(int whitePlayerId, int blackPlayerId)
    {
        try
        {
            // Use the repository to create a new game
            Game? _createdGame = await _gameRepository.CreateGameAsync(whitePlayerId, blackPlayerId);

            // Return the created game with a 201 Created response
            if (_createdGame != null)
            {
                return _createdGame;
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
        var _gameId = move.gameId;
        var _player = move.fromPlayerId;
        var _subMove = move.move;
        var _subTime = move.dateSubmitted;

        var (_isValid, _errorMessage) = await _gameRepository.ValidateMoveAsync(_gameId, _player);
        if (!_isValid)
            return Results.Problem(
                    type: "Bad Request",
                    title: "Validation Error",
                    detail: _errorMessage,
                    statusCode: StatusCodes.Status400BadRequest)
                as IActionResult;

        ChessMove _chessMove = moveRepository.FromLongAlgebraicNotation(move.move);
        PlayerRole _playerRole = await GetPlayerColor(_gameId, _player);
        moveRepository.AddMoveToDbAsync(_chessMove.StartPosition, _chessMove.EndPosition, _playerRole);

        return CreatedAtAction(nameof(GetGame), new { id = _gameId }, _subMove);
    }
}