using TerraRun.Services;
using TerraRun.Models;

namespace TerraRun.Pages;

[QueryProperty(nameof(TournamentId), "TournamentId")]
public partial class LeaderboardPage : ContentPage
{
    private readonly TournamentsService _tournamentsService;
    
    private string _tournamentId = string.Empty;
    public string TournamentId
    {
        get => _tournamentId;
        set
        {
            _tournamentId = value;
            if (int.TryParse(value, out var id))
            {
                LoadLeaderboard(id);
            }
        }
    }

    public LeaderboardPage()
    {
        InitializeComponent();
        _tournamentsService = new TournamentsService();
    }

    private async void LoadLeaderboard(int tournamentId)
    {
        try 
        {
            var leaders = await _tournamentsService.GetLeaderboard(tournamentId);
            LeaderboardList.ItemsSource = leaders;
        }
        catch (Exception)
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить таблицу лидеров", "ОК");
        }
    }
}