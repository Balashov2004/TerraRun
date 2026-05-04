using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Data;
using TerraRun.Api.DTO;
using TerraRun.Api.Models;
using TerraRun.Api.Services;

namespace TerraRun.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IGeoCellService _geoCellService;

    public RunsController(ApplicationDbContext context, IGeoCellService geoCellService)
    {
        _context = context;
        _geoCellService = geoCellService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartRun([FromBody] int userId)
    {
        var run = new Run { UserId = userId, Start = DateTime.UtcNow };
        _context.Add(run);
        await _context.SaveChangesAsync();
        return Ok(new RunResponseDto(run.Id));
    }

    [HttpPost("{runId}/stop")]
    public async Task<IActionResult> StopRun(int runId)
    {
        var run = await _context.Runs.FindAsync(runId);
        if (run == null) return NotFound();
        run.End = DateTime.UtcNow; 
    
        _context.Runs.Update(run);
        await _context.SaveChangesAsync();
    
        return Ok();
    }

    [HttpPost("{runId}/point")]
    public async Task<IActionResult> AddPoint(int runId, [FromBody] RunDto dto)
    {
        var point = new RunPoint
        {
            RunId = runId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Timestamp = DateTime.UtcNow
        };
        _context.RunPoints.Add(point);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{runId}/capture")]
    public async Task<IActionResult> CapturePoint(int runId, [FromBody] RunDto dto)
    {
        var h3IndexString = _geoCellService.GetCellIndex(dto.Latitude, dto.Longitude);
        var boundary = _geoCellService.GetBoundary(h3IndexString);

        var run = await _context.Runs.FindAsync(runId);
        if (run == null) return NotFound("Run not found");

        var userId = run.UserId;
        var cell = await _context.CapturedCells.FindAsync(h3IndexString);

        if (cell == null)
        {
            _context.CapturedCells.Add(new CapturedCell
            {
                H3Index = h3IndexString,
                OwnerUserId = userId,
                CapturedAt = DateTime.UtcNow
            });
        }
        else
        {
            cell.OwnerUserId = userId;
            cell.CapturedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { CellId = h3IndexString, OwnerUserId = userId, Boundary = boundary });
    }
    
    [HttpGet("captured-cells")] 
    public async Task<IActionResult> GetAllCells()
    { 
        var cellsFromDb = await _context.CapturedCells
            .Select(c => new { c.H3Index, c.OwnerUserId })
            .ToListAsync();
        
        var result = cellsFromDb.Select(c => new {
            CellId = c.H3Index,
            c.OwnerUserId,
            Boundary = _geoCellService.GetBoundary(c.H3Index)
        }).ToList();

        return Ok(result);
    }
    
    [HttpGet("stats/{userId}")]
    public async Task<IActionResult> GetStats(int userId)
    {
        var cellsCount = await _context.CapturedCells
            .CountAsync(c => c.OwnerUserId == userId);
        const double areaCell = 307.09d;
        var totalArea = areaCell * cellsCount;
        return Ok(new {
            CellsCount = cellsCount, 
            TotalAreaMeters = totalArea });
    }
}