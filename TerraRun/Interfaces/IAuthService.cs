using TerraRun.Models;

namespace TerraRun.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto?> Register(string username, string email, string password);
    Task<UserResponseDto?> Login(string username, string password);
}