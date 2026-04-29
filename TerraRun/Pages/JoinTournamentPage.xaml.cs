using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TerraRun.Pages;

public partial class JoinTournamentPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public JoinTournamentPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient {BaseAddress = new Uri("http://10.0.2.2:5000/")};
    }

    private async void OnJoinClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeEntry.Text)) return;
        
        var joinData = new
        {
            JoinCode = CodeEntry.Text,
            UserId = UserSession.LoggedInUserId
        };

        var response = await _httpClient.PostAsJsonAsync("api/Tournaments/join", joinData);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            await DisplayAlert("Ура!", result["message"], "OK");
            await Shell.Current.GoToAsync("..");
        }

        else
        {
            var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            await DisplayAlert("Ошибка",  error["message"], "OK");
        }
    }
}