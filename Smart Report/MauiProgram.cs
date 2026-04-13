using Microsoft.Extensions.Logging;
using Smart_Report.Services;

namespace Smart_Report;

public static class MauiProgram
{
    // ✅ 让页面里可以用 MauiProgram.Services 获取 DI 容器
    public static IServiceProvider Services { get; private set; } = default!;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // 保留你原来的字体
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 保留你原来的 DI 注册写法
        builder.Services.AddSingleton<Data.AppDb>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<NativeFeedbackService>();

        var app = builder.Build();

        // ✅ 保存 IServiceProvider（修复 CS0117）
        Services = app.Services;

        return app;
    }
}