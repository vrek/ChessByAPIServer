using System.ComponentModel.DataAnnotations;

namespace ChessByAPIServer.Models;

public class User
{
    // Primary key for the user entity
    [Key] public int Id { get; set; }

    [MaxLength(50)] public string UserName { get; set; } = string.Empty;

    [MaxLength(50)] public string Email { get; set; } = string.Empty;

    [MaxLength(50)] public string Password { get; set; }

    // Navigation properties for games played as White and Black
    public virtual ICollection<Game>? WhiteGames { get; set; } = []; // Correct initialization
    public virtual ICollection<Game>? BlackGames { get; set; } = []; // Correct initialization

    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
}