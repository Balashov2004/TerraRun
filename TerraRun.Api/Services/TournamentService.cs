using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Data;
using TerraRun.Api.DTO;
using TerraRun.Api.Models;

namespace TerraRun.Api.Services;

public interface ITournamentService
{
    Task<string> CreateAsync(TournamentCreateDto dto);
    Task<(bool Ok, string Message)> JoinAsync(JoinCodeDto dto);
    Task<List<TournamentListItemDto>> GetMyAsync(int userId);
    Task<CapturedCellDto?> CaptureCellAsync(int tournamentId, int userId, RunDto dto);
    Task<List<CapturedCellDto>> GetTournamentCellsAsync(int tournamentId);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int tournamentId);
}

public class TournamentService : ITournamentService
{
    private readonly ApplicationDbContext _context;
    private readonly IGeoCellService _geoCellService;

    public TournamentService(ApplicationDbContext context, IGeoCellService geoCellService)
    {
        _context = context;
        _geoCellService = geoCellService;
    }

    public async Task<string> CreateAsync(TournamentCreateDto dto)
    {
        var joinCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var tournament = new Tournament
        {
            Name = dto.Name,
            StartTime = dto.StartTime.ToUniversalTime(),
            EndTime = dto.EndTime.ToUniversalTime(),
            JoinCode = joinCode
        };

        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();
        return joinCode;
    }

    public async Task<(bool Ok, string Message)> JoinAsync(JoinCodeDto dto)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.JoinCode == dto.JoinCode.ToUpperInvariant());
        if (tournament is null)
            return (false, "Турнир с таким кодом не найден");

        if (DateTime.UtcNow > tournament.EndTime.ToUniversalTime())
            return (false, "Этот турнир уже завершен");

        var alreadyJoined = await _context.UserTournaments
            .AnyAsync(ut => ut.UserId == dto.UserId && ut.TournamentId == tournament.Id);
        if (alreadyJoined)
            return (false, "Вы уже участвуете в этом турнире");

        _context.UserTournaments.Add(new UserTournament
        {
            UserId = dto.UserId,
            TournamentId = tournament.Id,
            CapturedCells = []
        });

        await _context.SaveChangesAsync();
        return (true, $"Успешно! Вы вступили в турнир: {tournament.Name}");
    }

    public async Task<List<TournamentListItemDto>> GetMyAsync(int userId)
    {
        return await _context.UserTournaments
            .Where(p => p.UserId == userId)
            .Include(p => p.Tournament)
            .Select(p => new TournamentListItemDto
            {
                Id = p.Tournament.Id,
                Name = p.Tournament.Name,
                StartTime = p.Tournament.StartTime,
                EndTime = p.Tournament.EndTime,
                JoinCode = p.Tournament.JoinCode,
                IsActive = DateTime.UtcNow >= p.Tournament.StartTime &&
                           DateTime.UtcNow <= p.Tournament.EndTime
            })
            .ToListAsync();
    }

    public async Task<CapturedCellDto?> CaptureCellAsync(int tournamentId, int userId, RunDto dto)
    {
        var participant = await _context.UserTournaments
            .FirstOrDefaultAsync(ut => ut.TournamentId == tournamentId && ut.UserId == userId);
        if (participant is null)
            return null;

        var cellId = _geoCellService.GetCellIndex(dto.Latitude, dto.Longitude);
        if (!participant.CapturedCells.Contains(cellId))
        {
            participant.CapturedCells.Add(cellId);
            _context.Entry(participant).Property(x => x.CapturedCells).IsModified = true;
            await _context.SaveChangesAsync();
        }

        return new CapturedCellDto
        {
            CellId = cellId,
            OwnerUserId = participant.UserId,
            Boundary = _geoCellService.GetBoundary(cellId)
        };
    }

    public async Task<List<CapturedCellDto>> GetTournamentCellsAsync(int tournamentId)
    {
        var participants = await _context.UserTournaments
            .Where(x => x.TournamentId == tournamentId)
            .Select(x => new TournamentParticipantCellsDto
            {
                UserId = x.UserId,
                CapturedCells = x.CapturedCells
            })
            .ToListAsync();

        var result = new List<CapturedCellDto>();
        foreach (var participant in participants)
        {
            foreach (var cellId in participant.CapturedCells)
            {
                result.Add(new CapturedCellDto
                {
                    CellId = cellId,
                    OwnerUserId = participant.UserId,
                    Boundary = _geoCellService.GetBoundary(cellId)
                });
            }
        }

        return result;
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int tournamentId)
    {
        var rows = await _context.UserTournaments
            .Where(x => x.TournamentId == tournamentId)
            .Join(_context.Users,
                ut => ut.UserId,
                u => u.Id,
                (ut, u) => new
                {
                    ut.UserId,
                    u.Username,
                    CellsCount = ut.CapturedCells.Count
                })
            .OrderByDescending(x => x.CellsCount)
            .ThenBy(x => x.Username)
            .ToListAsync();

        return rows.Select((x, idx) => new LeaderboardEntryDto
            {
                Rank = idx + 1,
                UserId = x.UserId,
                UserName = x.Username,
                CellsCount = x.CellsCount
            })
            .ToList();
    }
}
