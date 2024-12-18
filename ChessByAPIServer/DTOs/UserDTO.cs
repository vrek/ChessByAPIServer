﻿using System.ComponentModel.DataAnnotations;
using ChessByAPIServer.Models;

namespace ChessByAPIServer.DTOs;

public class UserDto
{
    [Key] public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public virtual ICollection<Game> WhiteGames { get; set; } = []; // Correct initialization
    public virtual ICollection<Game> BlackGames { get; set; } = []; // Correct initialization
}

public class CreateUserDTO
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}