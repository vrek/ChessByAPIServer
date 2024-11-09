using ChessByAPIServer.Contexts;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserRepository userRepo, ChessDbContext context) : Controller
{
    private readonly ChessDbContext _context = context;
    private readonly IUserRepository _userRepo = userRepo;

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        var _users = await _userRepo.GetAll();
        if (_users == null || _users.Count == 0) return NotFound();
        return Ok(_users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetbyId(int id)
    {
        var _user = await _userRepo.GetbyIdAsync(id);
        if (_user == null) return NotFound();
        return Ok(_user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] User? user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var _result = await _userRepo.AddUser(user);
        if (_result == null) return Conflict("User with the same email or username already exists.");

        return CreatedAtAction(nameof(GetbyId), new { id = _result.Id }, _result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var _isDeleted = await _userRepo.DeleteUser(id);
        if (!_isDeleted) return NotFound();
        return NoContent();
    }
}