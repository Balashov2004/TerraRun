namespace TerraRun.Api.DTO;

public class TournamentCreateDto
{
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class JoinCodeDto
{
    public string JoinCode { get; set; } = string.Empty;
    public int UserId { get; set; }
}