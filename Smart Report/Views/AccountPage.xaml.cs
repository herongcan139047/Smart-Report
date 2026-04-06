using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class AccountPage : ContentPage
{
    private readonly AuthService _auth;

    public AccountPage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NameLabel.Text = $"Name: {_auth.CurrentName}";
        EmailLabel.Text = $"Email: {_auth.CurrentEmail}";
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        _auth.Logout();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}