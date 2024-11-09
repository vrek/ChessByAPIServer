using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;

namespace ChessByAPIServer.Mapper;

public static class UserMapper
{
    public static UserDto ToUserDto(this User userModel)
    {
        return new UserDto { Id = userModel.Id, UserName = userModel.UserName, Email = userModel.Email };
    }

    public static User CreateUserfromUserDto(this UserDto user)
    {
        return new User { UserName = user.UserName, Email = user.Email };
    }
}