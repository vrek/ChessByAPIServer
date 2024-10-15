using System.ComponentModel.DataAnnotations;

namespace ChessByAPIServer.Models;

public class GameMove
{
    [Key]
    public int Id { get; set; }

    // Game GUID that this move belongs to
    public Guid GameGuid { get; set; }

    // Move number (e.g., 1 for the first move, 2 for the second, etc.)
    public int MoveNumber { get; set; }

    // Move notation (e.g., "e4", "Nf3")
    public string MoveNotation { get; set; }

    // Navigation property to the Game entity
    public Game Game { get; set; }
}
