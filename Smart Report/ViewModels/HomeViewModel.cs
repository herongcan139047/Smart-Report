using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart_Report.Services;

namespace Smart_Report.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    [ObservableProperty] private string welcomeText = "Welcome";

    public HomeViewModel(AuthService auth)
    {
        _auth = auth;
    }

    public async Task RefreshAsync()
    {
        var user = await _auth.GetCurrentUserAsync();
        WelcomeText = user == null ? "Welcome" : $"Welcome, {user.DisplayName}";
    }

    [RelayCommand]
    private async Task GoTodosAsync() => await Shell.Current.GoToAsync("//todos");

    [RelayCommand]
    private async Task GoReportAsync() => await Shell.Current.GoToAsync("//report");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
