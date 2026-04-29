namespace TerraRun.Pages;

public partial class MenuPage :  ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private async void OnCreateTournamentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateTournamentPage));
    }

    private async void OnJoinTournamentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(JoinTournamentPage));
    }

    private async void OnMyTournamentsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MyTournamentsPage));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); 
    }
}