namespace TerraRun.Api.DTO;

public class RunDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
public record RunResponseDto(int Id);

