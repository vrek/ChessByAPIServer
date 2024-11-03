﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessByAPIServer.Models;
using ChessByAPIServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ChessByAPIServer;

public class ChessBoardRepositoryTests
{
    private readonly ChessDbContext _context;
    private readonly ChessBoardRepository _repository;
    private readonly Guid _gameId;

    public ChessBoardRepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(databaseName: "ChessTestDb")
            .Options;

        _context = new ChessDbContext(options);
        _repository = new ChessBoardRepository();
        _gameId = Guid.NewGuid();

        // Initialize the board for testing
        _repository.InitializeChessBoard(_context, _gameId).Wait();
    }

    [Fact]
    public async Task GetAllPositionsAsync_ShouldReturnAllPositionsForGame()
    {
        // Act
        var positions = await _repository.GetAllPositionsAsync(_context, _gameId);

        // Assert
        Assert.NotNull(positions);
        Assert.Equal(64, positions.Count); // Chessboard has 64 squares
        Assert.Contains(positions, p => p.Position == "e2" && p.Piece == "Pawn" && p.GameId == _gameId);
        Assert.Contains(positions, p => p.Position == "e7" && p.Piece == "Pawn" && p.GameId == _gameId);
    }

    [Theory]
    [InlineData("e4", "Knight","white", false)]  // Adding a Knight to e4
    [InlineData("d4", null,"white", true)]       // Clearing the piece at d4
    public async Task UpdatePositionAsync_ShouldUpdateSpecificPosition(string position, string? newPiece, string? color, bool expectedIsEmpty)
    {
        // Act
        var updateResult = await _repository.UpdatePositionAsync(_context, _gameId, position, newPiece, color);

        // Assert the update result
        Assert.True(updateResult);

        // Verify that the position has been updated
        var updatedPosition = await _context.ChessPositions
            .FirstOrDefaultAsync(p => p.GameId == _gameId && p.Position == position);

        Assert.NotNull(updatedPosition);
        Assert.Equal(newPiece, updatedPosition.Piece);
        Assert.Equal(expectedIsEmpty, updatedPosition.IsEmpty);
    }

    [Fact]
    public async Task UpdatePositionAsync_ShouldReturnFalseIfPositionNotFound()
    {
        // Arrange
        string nonExistentPosition = "z9"; // Invalid position

        // Act
        var result = await _repository.UpdatePositionAsync(_context, _gameId, nonExistentPosition, "Rook", "white");

        // Assert
        Assert.False(result); // Expect false as position does not exist
    }

    // Cleanup after each test
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
