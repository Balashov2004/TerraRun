using TerraRun.Services;

namespace TerraRun.Pages;

public partial class RegisterPage
{
    private readonly AuthService _authService;

    public RegisterPage()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try 
        {
            var success = await _authService.Register(UsernameEntry.Text, EmailEntry.Text, PasswordEntry.Text);
            if (success != null)
            {
                await DisplayAlert("good", "good", "ОК");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Ошибка", "Сервер вернул ошибку. Проверь логи API.", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Критическая ошибка", ex.Message, "ОК");
        }
    }
}