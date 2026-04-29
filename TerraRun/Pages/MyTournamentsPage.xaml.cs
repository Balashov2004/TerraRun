using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TerraRun.Pages;

public partial class MyTournamentsPage : ContentPage
{
    private HttpClient _httpClient;
    public MyTournamentsPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5000/") };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadTournaments();
    }

    private async Task LoadTournaments()
    {
        try
        {
            var tournaments = await _httpClient
                .GetFromJsonAsync<List<TournamentViewModel>>
                    ($"api/Tournaments/my/{UserSession.LoggedInUserId}");
            TournamentsList.ItemsSource = tournaments;
        }
        catch (Exception ex)
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
}

public class TournamentViewModel
{
    public string Name { get; set; }
    public string JoinCode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
}