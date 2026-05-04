namespace TerraRun.Api.DTO;

public class TournamentListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
}

public class BoundaryPointDto
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}

public class CapturedCellDto
{
    public string CellId { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
    public List<BoundaryPointDto> Boundary { get; set; } = [];
}

public class TournamentParticipantCellsDto
{
    public int UserId { get; set; }
    public List<string> CapturedCells { get; set; } = [];
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CellsCount { get; set; }
}
