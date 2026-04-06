using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;

    public RegisterPage()
    {
        InitializeComponent();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
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
            await DisplayAlert("OK", msg, "OK");
            await Navigation.PopAsync();
        }
    }
}