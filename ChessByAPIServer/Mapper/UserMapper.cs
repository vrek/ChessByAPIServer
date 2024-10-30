using ChessByAPIServer.DTOs;
using ChessByAPIServer.Models;

namespace ChessByAPIServer.Mapper;

public static class UserMapper
{
    public static UserDTO ToUserDTO(this User userModel)
    {
        return new UserDTO { Id = userModel.Id, UserName = userModel.UserName, Email = userModel.Email };
    }

    public static User CreateUserfromUserDTO(this UserDTO user)
    {
        return new User { UserName = user.UserName, Email = user.Email };
    }
}