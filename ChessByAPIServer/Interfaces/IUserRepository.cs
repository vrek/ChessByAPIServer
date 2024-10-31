using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer;

public interface IUserRepository : IDisposable
{
    Task<User?> AddUser([FromBody] User? user);
    Task<List<UserDTO>?> GetAll(); // Change this to return List<UserDTO>
    Task<User> GetbyId(int id);
    Task<bool> DeleteUser(int id);

    void Dispose();

    //Task<User?> AddUserAsync(User user);
}