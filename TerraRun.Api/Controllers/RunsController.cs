using H3;
using H3.Extensions;
using H3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Data;
using TerraRun.Api.DTO;
using TerraRun.Api.Models;

namespace TerraRun.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public RunsController(ApplicationDbContext context)
    {
        _context = context;
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
        var latRad = dto.Latitude * (Math.PI / 180.0);
        var lonRad = dto.Longitude * (Math.PI / 180.0);
        var point = new GeoCoord(latRad, lonRad);
        var indexObj = H3Index.FromGeoCoord(point, 12);
        var h3IndexString = indexObj.ToString();
        var boundary = indexObj.GetCellBoundaryVertices()
            .Select(v => new { 
                lat = v.Latitude * (180.0 / Math.PI),
                lon = v.Longitude * (180.0 / Math.PI), 
            })
            .ToList();

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
        return Ok(new { CellId = h3IndexString, Owner = userId, Boundary = boundary });
    }
    
    [HttpGet("captured-cells")] 
    public async Task<IActionResult> GetAllCells()
    { 
        var cellsFromDb = await _context.CapturedCells
            .Select(c => new { c.H3Index, c.OwnerUserId })
            .ToListAsync();
        
        var result = cellsFromDb.Select(c => new {
            CellId = c.H3Index,
            OwnerUserId = c.OwnerUserId,
            Boundary = GetBoundaryForIndex(c.H3Index)
        }).ToList();

        return Ok(result);
    }
    
    private List<object> GetBoundaryForIndex(string h3IndexString)
    {
        var indexObj = new H3Index(h3IndexString);
        return indexObj.GetCellBoundaryVertices()
            .Select(v => new { 
                lat = v.Latitude * (180.0 / Math.PI),
                lon = v.Longitude * (180.0 / Math.PI), 
            })
            .ToList<object>();
    }
}