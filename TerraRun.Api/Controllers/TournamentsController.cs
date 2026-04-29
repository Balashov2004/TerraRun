
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Data;
using TerraRun.Api.DTO;
using TerraRun.Api.Models;

namespace TerraRun.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    
    private readonly ApplicationDbContext _context;
    
    public TournamentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTournamen([FromBody] TournamentCreateDto dto)
    {
        var joinCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

        var tournament = new Tournament
        {
            Name = dto.Name,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            JoinCode = joinCode,
        };
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Турнир создан, сохраните код: {joinCode}"});
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinTournament([FromBody] JoinCodeDto dto)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.JoinCode == dto.JoinCode.ToUpper());
        if (tournament == null)
        {
            return NotFound(new {Message = "Турнир с таким кодом не найден"} );
        }
        if (DateTime.UtcNow > tournament.EndTime.ToUniversalTime())
        {
            return BadRequest(new { Message = "Этот турнир уже завершен" });
        }
        var alreadyJoined = await _context.UserTournaments
            .AnyAsync(ut => ut.UserId == dto.UserId && ut.TournamentId == tournament.Id);

        if (alreadyJoined)
        {
            return BadRequest(new { Message = "Вы уже участвуете в этом турнире" });
        }
        var participant = new UserTournament
        {
            UserId = dto.UserId,
            TournamentId = tournament.Id,
            CapturedCells = new List<string>() 
        };
        
        _context.UserTournaments.Add(participant);
        _context.SaveChanges();
        return Ok(new { Message = $"Успешно! Вы вступили в турнир: {tournament.Name}" });
    }

    [HttpGet("my/{userId}")]
    public async Task<IActionResult> GetMyTournaments(int userId)
    {
        var myTournaments = await _context.UserTournaments
            .Where(p => p.UserId == userId)
            .Include(p => p.Tournament)
            .Select(p => new
            {
                Name = p.Tournament.Name,
                StartTime = p.Tournament.StartTime,
                EndTime = p.Tournament.EndTime,
                JoinCode = p.Tournament.JoinCode,
                IsActive = DateTime.UtcNow >= p.Tournament.StartTime && 
                           DateTime.UtcNow <= p.Tournament.EndTime
            })
            .ToListAsync();
        
        return Ok(myTournaments);
    }
    
}
