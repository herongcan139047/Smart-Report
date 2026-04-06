using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Services;
using Smart_Report.Views;

namespace Smart_Report;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new ContentPage();
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        var auth = MauiProgram.Services.GetRequiredService<AuthService>();
        await auth.RestoreSessionAsync();

        if (auth.CurrentUser != null)
        {
            // 已登录，进入主界面
            MainPage = new AppShell();
        }
        else
        {
            // 未登录，先进入登录页
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}