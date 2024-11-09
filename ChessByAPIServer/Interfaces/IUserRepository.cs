using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer;

public interface IUserRepository : IDisposable
{
    Task<User?> AddUser([FromBody] User? user);
    Task<List<UserDto>?> GetAll(); // Change this to return List<UserDTO>
    Task<User?> GetbyIdAsync(int id);
    Task<bool> DeleteUser(int id);

    new void Dispose();

    //Task<User?> AddUserAsync(User user);
}