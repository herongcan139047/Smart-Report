using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly NativeFeedbackService _feedback;

    public LoginPage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
        _feedback = MauiProgram.Services.GetRequiredService<NativeFeedbackService>();
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
            await _feedback.ErrorVibrateAsync();
            return;
        }

        await _feedback.TapAsync();
        await _feedback.SuccessVibrateAsync();

        // 登录成功后进入主界面
        Application.Current!.MainPage = new AppShell();
    }

    private async void OnGoRegisterClicked(object sender, EventArgs e)
    {
        await _feedback.TapAsync();
        await Navigation.PushAsync(new RegisterPage());
    }
}