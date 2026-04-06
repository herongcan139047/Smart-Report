using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class HomePage : ContentPage
{
    private readonly AuthService _auth;

    public HomePage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_auth.CurrentUser == null)
        {
            AccountNameLabel.Text = "Account: Not logged in";
            AccountEmailLabel.Text = "Email: -";
            return;
        }

        AccountNameLabel.Text = $"Account: {_auth.CurrentName}";
        AccountEmailLabel.Text = $"Email: {_auth.CurrentEmail}";
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        _auth.Logout();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }

    private async void OnGoRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }

    private async void OnGoTodoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//todos");
    }

    private async void OnGoReportClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//report");
    }
}