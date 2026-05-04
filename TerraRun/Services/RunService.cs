using System.Net.Http.Json;
using TerraRun.Models;

namespace TerraRun.Services;

public class RunService : IRunService
{
    private readonly HttpClient _httpClient;

    public RunService()
    {
        _httpClient = ApiHttpClientProvider.Instance;
    }

    public async Task<int?> StartRun(int userId)
    {
        var response = await _httpClient.PostAsJsonAsync("Runs/start", userId);
        if (response.IsSuccessStatusCode)
        {
            var run = await response.Content.ReadFromJsonAsync<RunResponseDto>();
            return run?.Id;
        }
        return null;
    }

    public async Task SavePoint(int runId, double lat, double lon)
    {
        var dto = new { Latitude = lat, Longitude = lon };
        await _httpClient.PostAsJsonAsync($"Runs/{runId}/point", dto);
    }

    public async Task StopRun(int runId)
    {
        await _httpClient.PostAsync($"Runs/{runId}/stop", null);
    }

    public async Task<CaptureResponse?> CaptureCell(int runId, double lat, double lon)
    {
        try
        {
            var dto = new { Latitude = lat, Longitude = lon };
            var response = await _httpClient.PostAsJsonAsync($"Runs/{runId}/capture", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CaptureResponse>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVICE ERROR] CaptureCell: {ex.Message}");
        }
        return null;
    }

    public async Task<List<CapturedCellDto>> GetAllCapturedCells()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CapturedCellDto>>("Runs/captured-cells")
                   ?? new List<CapturedCellDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVICE ERROR] GetAllCapturedCells: {ex.Message}");
            return new List<CapturedCellDto>();
        }
    }

    public async Task<UserStatsDto?> GetStats(int userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserStatsDto>($"Runs/stats/{userId}");
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVICE ERROR] GetStats: {e.Message}");
        }
        return null;
    }
}