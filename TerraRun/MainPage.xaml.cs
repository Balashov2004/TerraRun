
using TerraRun.Pages;
using TerraRun.Services;

namespace TerraRun;

public partial class MainPage
{
    private readonly AuthService _authService;
    
    public MainPage()
    {
        InitializeComponent();
        _authService = new AuthService();
    }
    
    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(LoginUsername.Text) || string.IsNullOrEmpty(LoginPassword.Text))
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "ОК");
            return;
        }

        var success = await _authService.Login(LoginUsername.Text, LoginPassword.Text);
        if (success != null)
        {
            UserSession.LoggedInUserId = success.Id;
            UserSession.UserName = success.Name;
            await Navigation.PushAsync(new MapPage());
        }
        else
        {
            await DisplayAlert("Упс", "Неверный логин или пароль", "ОК");
        }
    }
}