using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer;
public interface IUserRepository
{
    Task<User?> AddUser([FromBody] User user);
    Task<List<UserDTO>?> GetAll();  // Change this to return List<UserDTO>
    Task<User> GetbyId(int id);
    Task<bool> DeleteUser(int id);
}