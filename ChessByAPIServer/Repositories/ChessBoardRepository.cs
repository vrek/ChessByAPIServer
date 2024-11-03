using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer.Repositories;

public class ChessBoardRepository : IChessBoardRepository
{
    public async Task<bool> InitializeChessBoard(ChessDbContext context, Guid gameId)
    {
        // Define the starting positions of pieces for white and black
        GeneratePieces(out var whitePieces, out var blackPieces);

        // Generate all positions for the chessboard (A1 to H8)
        var allPositions = GenerateAllPositions();

        // List to store all chess positions to be added
        List<ChessPosition> chessPositions = new();

        // Add white pieces with color
        AddPieces(gameId, whitePieces, chessPositions, "White");
        
        // Add black pieces with color
        AddPieces(gameId, blackPieces, chessPositions, "Black");

        GenerateEmptyPositions(gameId, whitePieces, blackPieces, allPositions, chessPositions);

        await AddAndSaveChanges(context, chessPositions);
        return true;
    }

    private static async Task AddAndSaveChanges(ChessDbContext context, List<ChessPosition> chessPositions)
    {
        context.ChessPositions.AddRange(chessPositions);
        await context.SaveChangesAsync();
    }

    private static void GenerateEmptyPositions(Guid gameId, Dictionary<string, string> whitePieces,
        Dictionary<string, string> blackPieces,
        List<string> allPositions, List<ChessPosition> chessPositions)
    {
        HashSet<string> occupiedPositions = new(whitePieces.Keys.Concat(blackPieces.Keys));
        foreach (var position in allPositions)
            if (!occupiedPositions.Contains(position))
                chessPositions.Add(new ChessPosition
                {
                    GameId = gameId,
                    Position = position,
                    IsEmpty = true,
                    Piece = null,
                    PieceColor = null // No piece, no color
                });
    }
    
    public static void AddPieces(Guid gameId, Dictionary<string, string> pieces,
        List<ChessPosition> chessPositions, string pieceColor)
    {
        foreach (var position in pieces)
            chessPositions.Add(new ChessPosition
            {
                GameId = gameId,
                Position = position.Key,
                IsEmpty = false,
                Piece = position.Value,
                PieceColor = pieceColor // Set the piece color
            });
    }
    
    public async Task<bool> UpdatePositionAsync(ChessDbContext context, Guid gameId, string position, string? newPiece = null, string? newPieceColor = null)
    {
        // Find the specific position to update in the database
        var targetPosition = await context.ChessPositions
            .FirstOrDefaultAsync(cp => cp.GameId == gameId && cp.Position == position);

        if (targetPosition == null)
        {
            // Position does not exist, return false to indicate failure
            return false;
        }

        // Update the piece and IsEmpty status
        targetPosition.Piece = newPiece;
        targetPosition.PieceColor = newPieceColor; // Update the color as well
        targetPosition.IsEmpty = newPiece == null;

        // Save only the changes made to targetPosition
        await context.SaveChangesAsync();

        return true; // Indicate success
    }

    private static void GeneratePieces(out Dictionary<string, string> whitePieces,
        out Dictionary<string, string> blackPieces)
    {
        whitePieces = new Dictionary<string, string>
        {
            { "a1", "Rook" }, { "b1", "Knight" }, { "c1", "Bishop" }, { "d1", "Queen" },
            { "e1", "King" }, { "f1", "Bishop" }, { "g1", "Knight" }, { "h1", "Rook" },
            { "a2", "Pawn" }, { "b2", "Pawn" }, { "c2", "Pawn" }, { "d2", "Pawn" },
            { "e2", "Pawn" }, { "f2", "Pawn" }, { "g2", "Pawn" }, { "h2", "Pawn" }
        };

        blackPieces = new Dictionary<string, string>
        {
            { "a8", "Rook" }, { "b8", "Knight" }, { "c8", "Bishop" }, { "d8", "Queen" },
            { "e8", "King" }, { "f8", "Bishop" }, { "g8", "Knight" }, { "h8", "Rook" },
            { "a7", "Pawn" }, { "b7", "Pawn" }, { "c7", "Pawn" }, { "d7", "Pawn" },
            { "e7", "Pawn" }, { "f7", "Pawn" }, { "g7", "Pawn" }, { "h7", "Pawn" }
        };
    }

    // Helper function to generate all board positions in algebraic notation (A1, A2, ..., H8)
    private static List<string> GenerateAllPositions()
    {
        string[] rows = { "1", "2", "3", "4", "5", "6", "7", "8" };
        string[] columns = { "a", "b", "c", "d", "e", "f", "g", "h" };

        List<string> positions = new();
        foreach (var col in columns)
        foreach (var row in rows)
            positions.Add($"{col}{row}");

        return positions;
    }

    public async Task<string?> GetPieceAtPositionAsync(ChessDbContext context, Guid gameId, string position)
    {
        var chessPosition = await context.ChessPositions
            .FirstOrDefaultAsync(cp => cp.GameId == gameId && cp.Position == position);

        return chessPosition?.Piece;
    }
    
    public async Task<string?> GetPieceColorAtPositionAsync(ChessDbContext context, Guid gameId, string position)
    {
        var chessPosition = await context.ChessPositions
            .FirstOrDefaultAsync(cp => cp.GameId == gameId && cp.Position == position);

        return chessPosition?.PieceColor; // Return the piece color
    }
    
    public async Task<List<ChessPosition>> GetAllPositionsAsync(ChessDbContext context, Guid gameId)
    {
        // Retrieve all positions for the specified gameId
        var chessPositions = await context.ChessPositions
            .Where(cp => cp.GameId == gameId)
            .ToListAsync();

        return chessPositions;
    }
    
    public async Task<bool> IsSquareOccupied(ChessDbContext context, Guid gameId, string position)
    {
        var targetPosition = await context.ChessPositions
            .FirstOrDefaultAsync(cp => cp.GameId == gameId && cp.Position == position);
        return targetPosition != null && !targetPosition.IsEmpty;
    }
}
