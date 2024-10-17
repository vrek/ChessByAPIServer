using ChessByAPIServer.DTOs;
using ChessByAPIServer.Mapper;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer;

public class UserRepository : IUserRepository
{
    private readonly ChessDbContext _context;
    public UserRepository(ChessDbContext context)
    {
        _context = context;
    }
    public async Task<User> AddUser([FromBody] User user)
    {
        // Check if the user already exists based on email or username (optional)
        User? existingUser = await _context.Users
            .FirstOrDefaultAsync(u => (u.Email == user.Email || u.UserName == user.UserName) && u.IsDeleted == false);

        if (existingUser != null)
        {
            return null;
        }
        user.IsDeleted = false;
        user.DateDeleted = null;


        // Add the new user to the database
        _ = await _context.Users.AddAsync(user);
        _ = await _context.SaveChangesAsync();

        return user; // Return the newly created user object
    }

    public async Task<List<UserDTO>> GetAll()
    {
        List<User> users = await _context.Users.ToListAsync();
        List<UserDTO> usersDTO = users.Select(u => u.ToUserDTO()).ToList();
        return usersDTO;
    }
    public async Task<User> GetbyId(int id)
    {
        Models.User? user = await _context.Users.FindAsync(id);
        return user;
    }
    public async Task<bool> DeleteUser(int id)
    {
        int _Id = id;
        User? _User = await GetbyId(_Id);
        if (_User == null)
        {
            return false;
        }
        _User.IsDeleted = true;
        _User.DateDeleted = DateTime.Now;
        _ = await _context.SaveChangesAsync();
        return true;
    }
}
