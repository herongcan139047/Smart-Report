using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly NativeFeedbackService _feedback;

    public RegisterPage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
        _feedback = MauiProgram.Services.GetRequiredService<NativeFeedbackService>();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        MsgLabel.Text = "";

        var (ok, msg) = await _auth.RegisterAsync(
            EmailEntry.Text ?? "",
            NameEntry.Text ?? "",
            PasswordEntry.Text ?? ""
        );

        MsgLabel.Text = msg;

        if (ok)
        {
            await _feedback.TapAsync();
            await _feedback.SuccessVibrateAsync();

            await DisplayAlert("OK", msg, "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await _feedback.ErrorVibrateAsync();
        }
    }
}