using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChessByAPIServer.Models;

public class ChessPosition
{
    [Key] public int Id { get; set; }

    public Guid GameId { get; set; }

    [Required]
    [MaxLength(2)]
    public required string Position { get; set; }

    public bool IsEmpty { get; set; }

    [MaxLength(10)] 
    public string? Piece { get; set; }
    
    [MaxLength(10)]
    public string? PieceColor { get; set; }

    [ForeignKey("GameId")]
    public virtual Game Game { get; set; }
}