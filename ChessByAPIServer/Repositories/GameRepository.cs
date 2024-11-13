using ChessByAPIServer.Contexts;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;
using PlayerRole = ChessByAPIServer.Enum.PlayerRole;

namespace ChessByAPIServer.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ChessBoardRepository _chessBoardRepository;
    private readonly ChessDbContext _context;

    public GameRepository(ChessDbContext context)
    {
        _context = context;
        _chessBoardRepository = new ChessBoardRepository();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        var game = await _context.Games
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .Include(g => g.ChessPositions)
            .Include(g => g.GameMoves)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null) return null;

        return game;
    }

    public ChessDbContext GetChessDbContext()
    {
        return _context;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Games.AnyAsync(g => g.Id == id);
    }

    public async Task<Game?> CreateGameAsync(int whitePlayerId, int blackPlayerId)
    {
        try
        {
            // Validate users and get their existence status
            await ValidateUser(whitePlayerId, "White player");
            await ValidateUser(blackPlayerId, "Black player");
        }
        catch (ArgumentException ex)
        {
            // Handle validation errors here, e.g., log the error, return an appropriate response
            throw new InvalidOperationException(ex.Message); // or handle it in another way
        } // Generate a new game ID

        var gameGuid = Guid.NewGuid();
        var whiteplayer = await _context.Users.FirstOrDefaultAsync(u => u != null && u.Id == whitePlayerId);
        var
            blackplayer =
                await _context.Users.FirstOrDefaultAsync(u =>
                    u != null &&
                    u.Id == blackPlayerId); // Create a new game instance using object initialization
        if (whiteplayer != null && blackplayer != null)
        {
            Game game = new(gameGuid, whiteplayer.Id, blackplayer.Id);
            if (game == null) throw new ArgumentNullException(nameof(game)); // Add the new game to the context
            _ = await _context.Games.AddAsync(game); // Save changes to the database
            _ = await _context.SaveChangesAsync(); // Initialize the chessboard with the new game
            await _chessBoardRepository.InitializeChessBoard(_context, gameGuid);
            await _context.SaveChangesAsync(); // Return the newly created game
            return game;
        }

        return null;
    }

    public async Task<List<Game>> GetGamesByPlayerIdAsync(int playerId, PlayerRole role = PlayerRole.All)
    {
        IQueryable<Game> query = _context.Games;

        // Filter based on the specified role
        if (role == PlayerRole.White)
            query = query.Where(x => x.WhitePlayerId == playerId);
        else if (role == PlayerRole.Black)
            query = query.Where(x => x.BlackPlayerId == playerId);
        else
            // If role is All, include both white and black games for the player
            query = query.Where(x => x.WhitePlayerId == playerId || x.BlackPlayerId == playerId);

        return await query.ToListAsync();
    }


    private async Task ValidateUser(int playerId, string playerRole)
    {
        // Check if the player exists
        var userExists = await _context.Users
            .AnyAsync(u => u != null && u.Id == playerId);

        if (!userExists) throw new ArgumentException($"{playerRole} with ID {playerId} does not exist.");

        // Check if the user is deleted
        var isDeleted = await _context.Users
            .AnyAsync(u => u != null && u.Id == playerId && u.IsDeleted);

        if (isDeleted) throw new ArgumentException($"{playerRole} with ID {playerId} is deleted.");
    }

    public async Task<Dictionary<string, object?>> GetChessPositionsDict(Game game)
    {
        // Load ChessPositions if they are not loaded
        if (!_context.Entry(game).Collection(g => g.ChessPositions).IsLoaded)
            await _context.Entry(game).Collection(g => g.ChessPositions).LoadAsync();
        // Create a dictionary to hold position data
        var positionsData = game.ChessPositions
            .Select(cp => new
            {
                cp.Position,
                Piece = cp.IsEmpty ? null : cp.Piece
            })
            .ToDictionary(x => x.Position, x => (object?)x.Piece);

        return positionsData;
    }

    public async Task<bool> TakeMoveAsync(Game game, string startPosition, string endPosition,
        PlayerRole playerRole)
    {
        var _piece = await _chessBoardRepository.GetPieceAtPositionAsync(_context, game.Id, startPosition);
        if (_piece == null) return false;
        var _moveRepo = new MoveRepository(game, _chessBoardRepository, this);
        if (_moveRepo == null) throw new ArgumentNullException(nameof(_moveRepo));
        var _validMove = await _moveRepo.IsValidMove(_piece, startPosition, endPosition, playerRole);
        if (!_validMove) return false;


        
        await _moveRepo.AddMoveToDbAsync(startPosition, endPosition, playerRole);
        await _chessBoardRepository.UpdatePositionAsync(_context, game.Id, endPosition, _piece, playerRole);
        await _chessBoardRepository.UpdatePositionAsync(_context, game.Id, startPosition);

        return true;
    }

    public async Task<string?> GetPieceAtPosition(Game game, string position)
    {
        var piece = await _chessBoardRepository.GetPieceAtPositionAsync(_context, game.Id, position);
        if (piece == null) return null;
        return piece;
    }

    public async Task<(bool isValid, string? errorMessage)> ValidateMoveAsync(Guid gameId, int playerId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null)
            return (false,
                "The specified game does not exist. Please create the game before making a move.");

        if (game.EndTime != null)
            return (false, "The game has already finished. You cannot make a move in a finished game.");

        if (playerId != game.WhitePlayerId && playerId != game.BlackPlayerId)
            return (false, "The player must be one of the participants in the game.");

        return (true, null); // Validation passed
    }
}