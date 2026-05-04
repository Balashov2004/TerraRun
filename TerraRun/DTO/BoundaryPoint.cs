namespace TerraRun.Models;

public record BoundaryPoint(double Lat, double Lon);
public class CapturedCellViewModel
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public List<BoundaryPoint> Boundary { get; set; } = [];
}