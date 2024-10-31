using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;
using PlayerRole = ChessByAPIServer.Enum.PlayerRole;

namespace ChessByAPIServer.Repositories;

public class GameRepository(ChessDbContext context) : IGameRepository
{
    public async Task<Game?> GetByIdAsync(Guid id)
    {
        var game = await context.Games
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .Include(g => g.ChessPositions)
            .Include(g => g.GameMoves)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null) return null;

        return game;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Games.AnyAsync(g => g.Id == id);
    }

    public async Task<Game> CreateGameAsync(int whitePlayerId, int blackPlayerId)
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
        }

        // Generate a new game ID
        var gameGuid = Guid.NewGuid();
        var whiteplayer = await context.Users.FirstOrDefaultAsync(u => u.Id == whitePlayerId);
        var blackplayer = await context.Users.FirstOrDefaultAsync(u => u.Id == blackPlayerId);
        // Create a new game instance using object initialization
        Game _game = new
        (
            gameGuid,
            whiteplayer.Id,
            blackplayer.Id
        );

        // Add the new game to the context
        _ = await context.Games.AddAsync(_game);

        // Save changes to the database
        _ = await context.SaveChangesAsync();

        // Initialize the chessboard with the new game ID
        ChessBoardRepository boardRepository = new();
        boardRepository.InitializeChessBoard(context, gameGuid);

        await context.SaveChangesAsync();

        // Return the newly created game
        return _game;
    }

    public async Task<List<Game>> GetGamesByPlayerIdAsync(int playerId, PlayerRole role = PlayerRole.All)
    {
        IQueryable<Game> query = context.Games;

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
        var userExists = await context.Users
            .AnyAsync(u => u.Id == playerId);

        if (!userExists) throw new ArgumentException($"{playerRole} with ID {playerId} does not exist.");

        // Check if the user is deleted
        var isDeleted = await context.Users
            .AnyAsync(u => u.Id == playerId && u.IsDeleted);

        if (isDeleted) throw new ArgumentException($"{playerRole} with ID {playerId} is deleted.");
    }

    public async Task<Dictionary<string, object?>> GetChessPositionsDict(Game game)
    {
        // Load ChessPositions if they are not loaded
        if (!context.Entry(game).Collection(g => g.ChessPositions).IsLoaded)
            await context.Entry(game).Collection(g => g.ChessPositions).LoadAsync();
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

    public async Task<bool> TakeMoveAsync(Game game, string startPosition, string endPosition)
    {
        var piece = await GetPieceAtPosition(game, startPosition);
        if (piece == null) return false;

        //TODO Determine if endPosition is valid move
        //TODO If move valid update DB with Move History
        //TODO If move valid update ChessPositions with new positions
        //TODO If all steps succeed return true, otherwise return false

        throw new NotImplementedException();
    }

    public async Task<string?> GetPieceAtPosition(Game game, string position)
    {
        ChessBoardRepository chessBoardRepository = new();
        var piece = await chessBoardRepository.GetPieceAtPositionAsync(context, game.Id, position);
        if (piece == null) return null;
        return piece;
    }
}