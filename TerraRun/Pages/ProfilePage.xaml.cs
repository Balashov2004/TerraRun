using TerraRun.Services;

namespace TerraRun.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly RunService _runService = new();
    public ProfilePage()
    {
        InitializeComponent();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserData();
    }
    private async Task LoadUserData()
    {
        UserNameLabel.Text = UserSession.UserName ;
        EmailLabel.Text = UserSession.UserEmail;

        try
        {
            var stats = await _runService.GetStats(UserSession.LoggedInUserId.Value);
            if (stats != null)
            {
                System.Diagnostics.Debug.WriteLine($"{stats.CellsCount} {stats.TotalAreaMeters}");
                CellsCountLabel.Text = stats.CellsCount.ToString();
                AreaLabel.Text = stats.TotalAreaMeters.ToString(); 
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Stats error: {ex.Message}");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var answer = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Нет");
        if (answer)
        {
            UserSession.LoggedInUserId = null;
            
            await Shell.Current.GoToAsync("//LoginPage"); 
        }
    }
}