using ChessByAPIServer.Controllers;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChessByAPI.Tests
{
    public class GameControllerTests
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly GameController _gameController;

        public GameControllerTests()
        {
            // Set up the mock repository
            _gameRepositoryMock = new Mock<IGameRepository>();

            // Create the controller and inject the mocked repository
            _gameController = new GameController(_gameRepositoryMock.Object);
        }

        // Test for GetGames action
        [Fact]
        public async Task GetGames_ReturnsOkResult_WithAListOfGames()
        {
            // Arrange: Set up the mock to return a list of games
            List<Game> mockGames = new()
            {
                new Game { Id = Guid.NewGuid(), WhitePlayerId = 1, BlackPlayerId = 2 },
                new Game { Id = Guid.NewGuid(), WhitePlayerId = 3, BlackPlayerId = 4 }
            };
            _ = _gameRepositoryMock.Setup(repo => repo.GetAllAsync())
                               .ReturnsAsync(mockGames);

            // Act: Call the GetGames action
            ActionResult<IEnumerable<Game>> result = await _gameController.GetGames();

            // Assert: Check if the result is OkObjectResult and contains the correct data
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            IEnumerable<Game?> returnedGames = Assert.IsAssignableFrom<IEnumerable<Game>>(okResult.Value);
            Assert.Equal(2, ((List<Game?>)returnedGames).Count);
        }

        // Test for GetGameById action
        [Fact]
        public async Task GetGameById_ReturnsOkResult_WithGame()
        {
            // Arrange: Set up a mock game
            Guid gameId = Guid.NewGuid();
            Game mockGame = new() { Id = gameId, WhitePlayerId = 1, BlackPlayerId = 2 };
            _ = _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId))
                               .ReturnsAsync(mockGame);

            // Act: Call the GetGameById action
            ActionResult<Game> result = await _gameController.GetGame(gameId);

            // Assert: Check if the result is OkObjectResult and contains the correct game
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Game returnedGame = Assert.IsType<Game>(okResult.Value);
            Assert.Equal(gameId, returnedGame.Id);
        }

        // Test for GetGameById action when game is not found
        [Fact]
        public async Task GetGameById_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange: Set up the mock to return null
            Guid gameId = Guid.NewGuid();
            _ = _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId))
                               .ReturnsAsync((Game)null);

            // Act: Call the GetGameById action
            ActionResult<Game> result = await _gameController.GetGame(gameId);

            // Assert: Check if the result is NotFoundResult
            _ = Assert.IsType<NotFoundResult?>(result.Result);
        }

        // Test for AddGame action
        [Fact]
        public async Task AddGame_ReturnsCreatedAtActionResult_WithGame()
        {
            // Arrange: Set up the mock game to be added
            Game newGame = new() { Id = Guid.NewGuid(), WhitePlayerId = 1, BlackPlayerId = 2 };
            _ = _gameRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Game>()))
                               .ReturnsAsync(newGame);

            // Act: Call the AddGame action
            ActionResult<Game> result = await _gameController.CreateGame(newGame);

            // Assert: Check if the result is CreatedAtActionResult
            CreatedAtActionResult createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Game returnedGame = Assert.IsType<Game>(createdAtActionResult.Value);
            Assert.Equal(newGame.Id, returnedGame.Id);
        }

        // Test for DeleteGame action
        [Fact]
        public async Task DeleteGame_ReturnsNoContent_WhenGameIsDeleted()
        {
            // Arrange: Set up the mock to return true when checking for existence
            Guid gameId = Guid.NewGuid();
            _ = _gameRepositoryMock.Setup(repo => repo.ExistsAsync(gameId))
                               .ReturnsAsync(true);
            _ = _gameRepositoryMock.Setup(repo => repo.DeleteAsync(gameId))
                               .Returns(Task.CompletedTask);

            // Act: Call the DeleteGame action
            IActionResult result = await _gameController.DeleteGame(gameId);

            // Assert: Check if the result is NoContentResult
            _ = Assert.IsType<NoContentResult?>(result);
        }

        // Test for DeleteGame action when game does not exist
        [Fact]
        public async Task DeleteGame_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange: Set up the mock to return false when checking for existence
            Guid gameId = Guid.NewGuid();
            _ = _gameRepositoryMock.Setup(repo => repo.ExistsAsync(gameId))
                               .ReturnsAsync(false);

            // Act: Call the DeleteGame action
            IActionResult result = await _gameController.DeleteGame(gameId);

            // Assert: Check if the result is NotFoundResult
            _ = Assert.IsType<NotFoundResult?>(result);
        }
    }
}
