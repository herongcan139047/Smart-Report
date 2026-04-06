namespace Smart_Report;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 可选：如果你有 Login/Register 页面
        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
    }
}