using ChessByAPIServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessByAPIServer.Interfaces;
public interface IUserController
{
    IActionResult AddUser([FromBody] User user);
    IActionResult GetAll();
    IActionResult GetbyId(int id);
}