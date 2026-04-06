using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart_Report.Services;

namespace Smart_Report.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    [ObservableProperty] private string displayName = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";

    public RegisterViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = "";

        try
        {
            var (ok, msg) = await _auth.RegisterAsync(Email, Password, DisplayName);
            if (!ok) { ErrorMessage = msg; return; }

            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
