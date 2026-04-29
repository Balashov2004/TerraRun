namespace TerraRun.Api.Models;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string JoinCode { get; set; } = string.Empty;
}