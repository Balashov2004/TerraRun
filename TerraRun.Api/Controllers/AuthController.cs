using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Data;
using TerraRun.Api.Models;
using TerraRun.Api.DTO;

namespace TerraRun.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("User c таким email уже такой существует");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password,
            Experience = 0
        };

        _context.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

        if (user == null)
        {
            return Unauthorized("Неверное имя пользователя или пароль");
        }

        return Ok(new { user.Id, user.Username, user.Email });
    }
    
    public record LoginRequest(string Username, string Password);
}