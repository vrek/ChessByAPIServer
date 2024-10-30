using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChessByAPIServer.Models;

public class Game
{
    // Constructor with default StartTime as DateTime.UtcNow
    public Game(Guid id, int whitePlayerId, int blackPlayerId)
    {
        Id = id;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
        StartTime = DateTime.UtcNow; // If startTime is null, default to DateTime.UtcNow
        EndTime = null; // EndTime can be null
        ChessPositions = new List<ChessPosition>(); // Initialize the collections

        GameMoves = new List<GameMove>();
    }

    [Key] public Guid Id { get; set; } // No default value needed here

    // Foreign key for the white player
    public int WhitePlayerId { get; set; } // Use consistent casing for properties

    // Foreign key for the black player
    public int BlackPlayerId { get; set; } // Use consistent casing for properties

    // Entry for start time of play
    public DateTime StartTime { get; set; } // No default value needed here; it's set in the constructor

    // Entry for end time of play
    public DateTime? EndTime { get; set; } // Nullable type

    // Navigation properties for the white and black players
    [ForeignKey("WhitePlayerId")] public virtual User WhitePlayer { get; set; }

    [ForeignKey("BlackPlayerId")] public virtual User BlackPlayer { get; set; }

    // A collection of chess positions for the game
    public virtual ICollection<ChessPosition> ChessPositions { get; set; } // Initialized in the constructor

    // A collection of moves for the game
    public virtual ICollection<GameMove> GameMoves { get; set; } // Initialized in the constructor
}