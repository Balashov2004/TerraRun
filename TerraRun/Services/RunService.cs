using System.Net.Http.Json;
using TerraRun.Models;

namespace TerraRun.Services;

public class RunService : IRunService
{
    private readonly HttpClient _httpClient;

    public RunService()
    {
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("http://10.0.2.2:5134/api/Runs/") 
        };
    }

    public async Task<int?> StartRun(int userId)
    {
        var response = await _httpClient.PostAsJsonAsync("start", userId);
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
        await _httpClient.PostAsJsonAsync($"{runId}/point", dto);
    }

    public async Task StopRun(int runId)
    {
        await _httpClient.PostAsync($"{runId}/stop", null);
    }

    public async Task<CaptureResponse?> CaptureCell(int runId, double lat, double lon)
    {
        try
        {
            var dto = new { Latitude = lat, Longitude = lon };
            var response = await _httpClient.PostAsJsonAsync($"{runId}/capture", dto);

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
            return await _httpClient.GetFromJsonAsync<List<CapturedCellDto>>("captured-cells") 
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
            return await _httpClient.GetFromJsonAsync<UserStatsDto>($"stats/{userId}");
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVICE ERROR] GetStats: {e.Message}");
        }
        return null;
    }
}