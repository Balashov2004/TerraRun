namespace TerraRun.Models;

public record RunResponseDto(int Id);

public record CaptureResponse(string CellId, int OwnerUserId, List<BoundaryPoint> Boundary);

public class CapturedCellDto
{
    public int OwnerUserId { get; set; }
    public List<BoundaryPoint> Boundary { get; set; } = new();
}

public class UserStatsDto 
{
    public int CellsCount { get; set; }
    public double TotalAreaMeters { get; set; }
}