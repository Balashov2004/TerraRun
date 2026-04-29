using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TerraRun.Pages;

public partial class CreateTournamentPage : ContentPage
{
    private readonly HttpClient _httpClient;
    
    public CreateTournamentPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient {BaseAddress = new Uri("http://10.0.2.2:5000/")};
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
        var tournamentDto = new
        {
            Name = NameEntry.Text,
            StartTime = start.ToUniversalTime(),
            EndTime = end.ToUniversalTime(),
        };
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Tournaments/create", tournamentDto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                await DisplayAlert("Успех", $"Сохрани {result["message"]}", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
    
                // Выводим этот текст — там будет написано, какое поле не нравится
                await DisplayAlert("Детали ошибки", errorBody, "OK");
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