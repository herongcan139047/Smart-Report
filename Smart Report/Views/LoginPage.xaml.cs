using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;

    public LoginPage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        MsgLabel.Text = "";

        var (ok, msg) = await _auth.LoginAsync(
            EmailEntry.Text ?? "",
            PasswordEntry.Text ?? ""
        );

        if (!ok)
        {
            MsgLabel.Text = msg;
            return;
        }

        // 登录成功后进入主界面
        Application.Current!.MainPage = new AppShell();
    }

    private async void OnGoRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}