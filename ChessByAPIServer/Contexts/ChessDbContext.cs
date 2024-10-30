using ChessByAPIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer;

public class ChessDbContext(DbContextOptions<ChessDbContext> options) : DbContext(options)
{
    // DbSets for the models
    public DbSet<User> Users { get; set; }
    public DbSet<ChessPosition> ChessPositions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameMove> GameMoves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuring the Game entity
        _ = modelBuilder.Entity<Game>()
            .HasKey(g => g.Id);

        // Relationships and additional configurations
        // Each user can play as White in many games
        _ = modelBuilder.Entity<Game>()
            .HasOne(g => g.WhitePlayer)
            .WithMany(u => u.WhiteGames) // Reference the WhiteGames collection
            .HasForeignKey(g => g.WhitePlayerId)
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascading deletes for players

        // Each user can play as Black in many games
        _ = modelBuilder.Entity<Game>()
            .HasOne(g => g.BlackPlayer)
            .WithMany(u => u.BlackGames) // Reference the BlackGames collection
            .HasForeignKey(g => g.BlackPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Each game can have many chess positions
        _ = modelBuilder.Entity<Game>()
            .HasMany(g => g.ChessPositions)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .IsRequired();

        // Each game can have many moves
        _ = modelBuilder.Entity<GameMove>()
            .HasOne(m => m.Game)
            .WithMany(g => g.GameMoves)
            .HasForeignKey(m => m.GameGuid);

        // ChessPosition: Position column should be required with max length of 2 (e.g., "A1", "B8")
        _ = modelBuilder.Entity<ChessPosition>()
            .Property(p => p.Position)
            .IsRequired()
            .HasMaxLength(2);

        // ChessPosition: Piece column should have a max length (e.g., "Pawn", "Rook", etc.)
        _ = modelBuilder.Entity<ChessPosition>()
            .Property(p => p.Piece)
            .HasMaxLength(10);

        // GameMove: MoveNotation should have max length (e.g., "e4", "Nf3")
        _ = modelBuilder.Entity<GameMove>()
            .Property(m => m.MoveNotation)
            .HasMaxLength(10);
    }
}