using TerraRun.Models;
using TerraRun.Services;

namespace TerraRun.Pages;

public partial class MyTournamentsPage : ContentPage
{
    private readonly TournamentsService _tournamentsService;

    public MyTournamentsPage()
    {
        InitializeComponent();
        _tournamentsService = new TournamentsService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadTournaments();
    }

    private async Task LoadTournaments()
    {
        try
        {
            var tournaments = await _tournamentsService.GetMyTournaments(UserSession.LoggedInUserId ?? 0);
            TournamentsList.ItemsSource = tournaments;
        }
        catch (Exception)
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить список", "OK");
        }
        finally
        {
            RefreshView.IsRefreshing = false;
        }
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadTournaments();
    }

    private async void OnTournamentClick(object sender, TappedEventArgs e)
    {
        if (e.Parameter is TournamentViewModel selectedTournament)
        {

            var navigationParameter = new Dictionary<string, object>
            {
                { "Tournament", selectedTournament }
            };

            await Shell.Current.GoToAsync(nameof(TournamentMapPage), navigationParameter);
        }
    }
}

