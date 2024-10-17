using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : Controller//, IUserRepository
{
    private readonly ChessDbContext _context;
    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        List<UserDTO> _users = await _userRepo.GetAll();
        return Ok(_users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetbyId(int id)
    {
        User _user = await _userRepo.GetbyId(id);
        if (_user == null)
        {
            return NotFound();
        }
        return Ok(_user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Handle validation here
        }

        User result = await _userRepo.AddUser(user);
        if (result == null)
        {
            return Conflict("User with the same email or username already exists.");
        }

        return CreatedAtAction(nameof(GetbyId), new { id = result.Id }, result);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        bool isDeleted = await _userRepo.DeleteUser(id);
        if (!isDeleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
