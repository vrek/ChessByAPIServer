using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChessByAPIServer.Models;

public class Game
{
    [Key]
    public Guid Id { get; set; }

    // Foreign key for the white player
    public int WhitePlayerId { get; set; }

    // Foreign key for the black player
    public int BlackPlayerId { get; set; }

    //Entry for start time of play
    public DateTime StartTime { get; set; }

    //Entry for end time of play
    public DateTime? EndTime { get; set; }

    // Navigation properties for the white and black players
    [ForeignKey("WhitePlayerId")]
    public virtual User WhitePlayer { get; set; }

    [ForeignKey("BlackPlayerId")]
    public virtual User BlackPlayer { get; set; }

    // A collection of chess positions for the game
    public virtual ICollection<ChessPosition>? ChessPositions { get; set; }

    // A collection of moves for the game
    public virtual ICollection<GameMove>? GameMoves { get; set; }
}
