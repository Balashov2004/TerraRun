namespace TerraRun.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserData();
    }
    private void LoadUserData()
    {
        UserNameLabel.Text = UserSession.UserName ;
        EmailLabel.Text = UserSession.UserEmail;
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