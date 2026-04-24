namespace TerraRun.Models;

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);