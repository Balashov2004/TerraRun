using System.Net.Http.Json;

namespace TerraRun.Services;

public record RunResponseDto(int Id);

public class RunService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://10.0.2.2:5134/api/Runs/";

    public RunService()
    {
        _httpClient = new  HttpClient();
    }

    public async Task<int?> StartRun(int userId)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}start", userId);
        if (response.IsSuccessStatusCode)
        {
            var run = await response.Content.ReadFromJsonAsync<RunResponseDto>();
            return run?.Id;
        }
        return null;
    }
    public async Task SavePoint(int runId, double lat, double lon)
    {
        var dto = new  { Latitude = lat, Longitude = lon };
        await _httpClient.PostAsJsonAsync($"{BaseUrl}{runId}/point", dto);
    }

    public async Task StopRun(int runId)
    {
        await _httpClient.PostAsync($"{BaseUrl}{runId}/stop", null);
    }

    public async Task<CaptureResponse?> CaptureCell(int runId, double lat, double lon)
    {
        try
        {
            var dto = new { Latitude = lat, Longitude = lon };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}{runId}/capture", dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CaptureResponse>();
                System.Diagnostics.Debug.WriteLine($"[SERVICE] Получено точек границы: {result?.Boundary?.Count ?? 0}");
                return result;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVICE ERROR] {ex.Message}");
        }
        return null;
    }
}

public record CaptureResponse(string CellId, int Owner, List<BoundaryPoint> Boundary);

public record BoundaryPoint(double Lat, double Lon);
















