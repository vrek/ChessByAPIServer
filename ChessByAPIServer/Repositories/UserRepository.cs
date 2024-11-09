using ChessByAPIServer.Contexts;
using ChessByAPIServer.DTOs;
using ChessByAPIServer.Mapper;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChessDbContext _context;

    public UserRepository(ChessDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AddUser([FromBody] User? user)
    {
        var _existingUser = await _context.Users
            .FirstOrDefaultAsync(u =>
                user != null && (u.Email == user.Email || u.UserName == user.UserName) &&
                u.IsDeleted == false);

        if (_existingUser != null) return null;

        if (user != null)
        {
            user.IsDeleted = false;
            user.DateDeleted = null;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        return null;
    }

    public async Task<List<UserDto>?> GetAll()
    {
        var _users = await _context.Users.ToListAsync();
        var _usersDto = _users.Select(u => u.ToUserDto()).ToList();
        return _usersDto;
    }

    public async Task<User?> GetbyIdAsync(int id)
    {
        User? _user = await _context.Users.FindAsync(id);
        if (_user == null || _user.IsDeleted) 
        {
            return null;
            
        }
        else
        {
            return _user;
        }
    }

    public async Task<bool> DeleteUser(int id)
    {
        var _user = await GetbyIdAsync(id);

        if (_user != null)
        {
            _user.IsDeleted = true;
            _user.DateDeleted = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<int> GetbyEmail(string email)
    {
        var _user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        return _user?.Id ?? 0;
    }
}