
using Microsoft.AspNetCore.Mvc;
using TerraRun.Api.DTO;
using TerraRun.Api.Services;

namespace TerraRun.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentsController(ITournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTournament([FromBody] TournamentCreateDto dto)
    {
        var joinCode = await _tournamentService.CreateAsync(dto);
        return Ok(new { Message = $"Турнир создан, сохраните код: {joinCode}"});
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinTournament([FromBody] JoinCodeDto dto)
    {
        var result = await _tournamentService.JoinAsync(dto);
        if (!result.Ok)
            return BadRequest(new { Message = result.Message });

        return Ok(new { Message = result.Message });
    }

    [HttpGet("my/{userId}")]
    public async Task<IActionResult> GetMyTournaments(int userId)
    {
        return Ok(await _tournamentService.GetMyAsync(userId));
    }

    [HttpGet("{id}/cells")]
    public async Task<IActionResult> GetCellsByTournament(int id)
    {
        return Ok(await _tournamentService.GetTournamentCellsAsync(id));
    }

    [HttpPost("{id}/capture/{userId}")]
    public async Task<IActionResult> CaptureCell(int id, int userId, [FromBody] RunDto dto)
    {
        var result = await _tournamentService.CaptureCellAsync(id, userId, dto);
        if (result is null)
            return NotFound(new { Message = "Вы не участник этого турнира" });

        return Ok(result);
    }

    [HttpGet("{id}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(int id)
    {
        return Ok(await _tournamentService.GetLeaderboardAsync(id));
    }
}
