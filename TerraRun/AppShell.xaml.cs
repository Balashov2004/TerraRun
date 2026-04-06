using TerraRun.Pages;
namespace TerraRun;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        try 
        {
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            System.Diagnostics.Debug.WriteLine("[DEBUG] Маршрут ProfilePage успешно зарегистрирован");
        }
        catch (Exception ex) 
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Ошибка регистрации роута: {ex.Message}");
        }
    }
}