using TerraRun.Pages;
namespace TerraRun;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        try 
        {
            Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(CreateTournamentPage), typeof(CreateTournamentPage));
            Routing.RegisterRoute(nameof(JoinTournamentPage), typeof(JoinTournamentPage));
            Routing.RegisterRoute(nameof(MyTournamentsPage), typeof(MyTournamentsPage));
            Routing.RegisterRoute(nameof(TournamentMapPage), typeof(TournamentMapPage));
            Routing.RegisterRoute(nameof(LeaderboardPage), typeof(LeaderboardPage));
            System.Diagnostics.Debug.WriteLine("[DEBUG] Маршрут ProfilePage успешно зарегистрирован");
        }
        catch (Exception ex) 
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Ошибка регистрации роута: {ex.Message}");
        }
    }
}