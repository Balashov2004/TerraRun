namespace TerraRun.Models;

public class TournamentCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class TournamentJoinRequest
{
    public string JoinCode { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class ApiMessageResponse
{
    public string Message { get; set; } = string.Empty;
}

public class LeaderboardEntryViewModel
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CellsCount { get; set; }
}
