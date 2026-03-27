namespace TerraRun.Api.DTO;

public class RunDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public record StartRunRequest(int UserId);

public record RunPointDto(double Latitude, double Longitude);

public record RunResponseDto(int Id);