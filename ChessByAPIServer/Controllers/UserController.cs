using ChessByAPIServer.DTOs;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Mapper;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : Controller, IUserController
{
    private readonly ChessDbContext _context;

    public UserController(ChessDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("")]
    public IActionResult GetAll()
    {
        List<UserDTO> users = _context.Users
        .ToList()
        .Select(u => u.ToUserDTO())
        .ToList();
        return Ok(users);
    }
    [HttpGet("{id}")]
    public IActionResult GetbyId(int id)
    {
        Models.User? user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public IActionResult AddUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if the user already exists based on email or username (optional)
        User? existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email || u.UserName == user.UserName);
        if (existingUser != null)
        {
            return Conflict("User with the same email or username already exists.");
        }

        // Add the new user to the database
        _ = _context.Users.Add(user);
        _ = _context.SaveChanges();

        // Return the newly created user object, or you could return just the ID
        return CreatedAtAction(nameof(GetbyId), new { id = user.Id }, user);
    }
}
