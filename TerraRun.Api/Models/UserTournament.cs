namespace TerraRun.Api.Models;

public class UserTournament
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public List<string> CapturedCells { get; set; } = new();
}