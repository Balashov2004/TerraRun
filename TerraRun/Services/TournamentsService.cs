using System.Net.Http.Json;
using TerraRun.Models;

namespace TerraRun.Services;

public class TournamentsService
{
    private readonly HttpClient _httpClient;

    public TournamentsService()
    {
        _httpClient = ApiHttpClientProvider.Instance;
    }

    public async Task<string?> CreateTournament(TournamentCreateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("Tournaments/create", request);
        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        return result?.Message;
    }

    public async Task<string?> JoinTournament(TournamentJoinRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("Tournaments/join", request);
        var result = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        return result?.Message;
    }

    public async Task<List<TournamentViewModel>> GetMyTournaments(int userId)
    {
        return await _httpClient.GetFromJsonAsync<List<TournamentViewModel>>($"Tournaments/my/{userId}") ?? [];
    }

    public async Task<CaptureResponse?> CaptureTournamentCell(int tournamentId, double lat, double lon)
    {
        try
        {
            var dto = new { Latitude = lat, Longitude = lon };
            var userId = UserSession.LoggedInUserId;
            var response = await _httpClient.PostAsJsonAsync($"Tournaments/{tournamentId}/capture/{userId}", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CaptureResponse>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error] Capture: {ex.Message}");
        }
        return null;
    }

    public async Task<List<CapturedCellDto>> GetCellsByTournament(int tournamentId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CapturedCellDto>>($"Tournaments/{tournamentId}/cells") ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error] GetCellsByTournament: {ex.Message}");
            return [];
        }
    }

    public async Task<List<LeaderboardEntryViewModel>> GetLeaderboard(int tournamentId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<LeaderboardEntryViewModel>>($"Tournaments/{tournamentId}/leaderboard") ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error] GetLeaderboard: {ex.Message}");
            return [];
        }
    }
}