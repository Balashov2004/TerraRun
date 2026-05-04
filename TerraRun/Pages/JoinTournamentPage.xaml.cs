using System;
using TerraRun.Models;
using TerraRun.Services;

namespace TerraRun.Pages;

public partial class JoinTournamentPage : ContentPage
{
    private readonly TournamentsService _tournamentsService;

    public JoinTournamentPage()
    {
        InitializeComponent();
        _tournamentsService = new TournamentsService();
    }

    private async void OnJoinClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeEntry.Text)) return;
        
        var joinData = new TournamentJoinRequest
        {
            JoinCode = CodeEntry.Text ?? string.Empty,
            UserId = UserSession.LoggedInUserId ?? 0
        };

        var message = await _tournamentsService.JoinTournament(joinData);
        if (!string.IsNullOrWhiteSpace(message))
        {
            await DisplayAlert("Ура!", message, "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Ошибка", "Не удалось вступить в турнир", "OK");
        }
    }
}