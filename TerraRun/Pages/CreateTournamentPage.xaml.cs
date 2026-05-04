using System;
using TerraRun.Models;
using TerraRun.Services;

namespace TerraRun.Pages;

public partial class CreateTournamentPage : ContentPage
{
    private readonly TournamentsService _tournamentsService;

    public CreateTournamentPage()
    {
        InitializeComponent();
        _tournamentsService = new TournamentsService();
    }


    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Ошибка", "Введите название турнира", "OK");
            return;
        };
        DateTime start = StartDatePicker.Date + StartTimePicker.Time;
        DateTime end = EndDatePicker.Date + EndTimePicker.Time;

        if (end <= start)
        {
            await DisplayAlert("Ошибка", "Турнир не может закончиться раньше, чем начнется", "OK");
            return;
        }
        var tournamentDto = new TournamentCreateRequest
        {
            Name = NameEntry.Text,
            StartTime = start.ToUniversalTime(),
            EndTime = end.ToUniversalTime(),
        };
        try
        {
            var message = await _tournamentsService.CreateTournament(tournamentDto);
            if (!string.IsNullOrWhiteSpace(message))
            {
                await DisplayAlert("Успех", message, "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось создать турнир", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.ToString(), "OK");
        }
        
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
    
}