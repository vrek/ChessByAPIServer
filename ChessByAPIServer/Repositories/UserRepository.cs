using ChessByAPIServer.DTOs;
using ChessByAPIServer.Mapper;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer;

public class UserRepository : IUserRepository, IDisposable
{
    private readonly ChessDbContext _context;

    public UserRepository(ChessDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AddUser([FromBody] User? user)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => (u.Email == user.Email || u.UserName == user.UserName) && u.IsDeleted == false);

        if (existingUser != null) return null;

        user.IsDeleted = false;
        user.DateDeleted = null;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<List<UserDTO>?> GetAll()
    {
        var users = await _context.Users.ToListAsync();
        var usersDto = users.Select(u => u.ToUserDTO()).ToList();
        return usersDto;
    }

    public async Task<User> GetbyId(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var _User = await GetbyId(id);
        if (_User == null) return false;

        _User.IsDeleted = true;
        _User.DateDeleted = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<int> GetbyEmail(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        return user?.Id ?? 0;
    }
}