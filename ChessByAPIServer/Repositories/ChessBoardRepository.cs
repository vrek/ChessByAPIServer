using ChessByAPIServer.Models;

namespace ChessByAPIServer.Repositories;

public class ChessBoardRepository
{
    public static void InitializeChessBoard(ChessDbContext context, Guid gameId)
    {
        // Define the starting positions of pieces for white and black
        Dictionary<string, string> whitePieces = new()
        {
            { "A1", "Rook" }, { "B1", "Knight" }, { "C1", "Bishop" }, { "D1", "Queen" },
            { "E1", "King" }, { "F1", "Bishop" }, { "G1", "Knight" }, { "H1", "Rook" },
            { "A2", "Pawn" }, { "B2", "Pawn" }, { "C2", "Pawn" }, { "D2", "Pawn" },
            { "E2", "Pawn" }, { "F2", "Pawn" }, { "G2", "Pawn" }, { "H2", "Pawn" }
        };

        Dictionary<string, string> blackPieces = new()
        {
            { "A8", "Rook" }, { "B8", "Knight" }, { "C8", "Bishop" }, { "D8", "Queen" },
            { "E8", "King" }, { "F8", "Bishop" }, { "G8", "Knight" }, { "H8", "Rook" },
            { "A7", "Pawn" }, { "B7", "Pawn" }, { "C7", "Pawn" }, { "D7", "Pawn" },
            { "E7", "Pawn" }, { "F7", "Pawn" }, { "G7", "Pawn" }, { "H7", "Pawn" }
        };

        // Generate all positions for the chessboard (A1 to H8)
        List<string> allPositions = GenerateAllPositions();

        // List to store all chess positions to be added
        List<ChessPosition> chessPositions = [];

        // Add white pieces
        foreach (KeyValuePair<string, string> position in whitePieces)
        {
            chessPositions.Add(new ChessPosition
            {
                GameId = gameId,
                Position = position.Key,
                IsEmpty = false,
                Piece = position.Value
            });
        }

        // Add black pieces
        foreach (KeyValuePair<string, string> position in blackPieces)
        {
            chessPositions.Add(new ChessPosition
            {
                GameId = gameId,
                Position = position.Key,
                IsEmpty = false,
                Piece = position.Value
            });
        }

        // Add empty positions
        HashSet<string> occupiedPositions = [.. whitePieces.Keys, .. blackPieces.Keys];
        foreach (string position in allPositions)
        {
            if (!occupiedPositions.Contains(position))
            {
                chessPositions.Add(new ChessPosition
                {
                    GameId = gameId,
                    Position = position,
                    IsEmpty = true,
                    Piece = null
                });
            }
        }

        // Add to the context and save changes
        context.ChessPositions.AddRange(chessPositions);
        _ = context.SaveChanges();
    }

    // Helper function to generate all board positions in algebraic notation (A1, A2, ..., H8)
    private static List<string> GenerateAllPositions()
    {
        string[] rows = ["1", "2", "3", "4", "5", "6", "7", "8"];
        string[] columns = ["A", "B", "C", "D", "E", "F", "G", "H"];

        List<string> positions = [];
        foreach (string? col in columns)
        {
            foreach (string? row in rows)
            {
                positions.Add($"{col}{row}");
            }
        }

        return positions;
    }
}

