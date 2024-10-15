using System.ComponentModel.DataAnnotations;

namespace ChessByAPIServer.Models;

public class User
{
    // Primary key for the user entity
    [Key]
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; }

    // Navigation properties for games played as White and Black
    public virtual ICollection<Game> WhiteGames { get; set; } = new List<Game>(); // Correct initialization
    public virtual ICollection<Game> BlackGames { get; set; } = new List<Game>(); // Correct initialization
}