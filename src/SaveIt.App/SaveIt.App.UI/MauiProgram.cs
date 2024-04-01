using Microsoft.Extensions.Logging;
using SaveIt.App.UI.Extensions;

namespace SaveIt.App.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Configuration.AddAppJsonConfiguration();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBlazorBootstrap();

        builder.Services.AddUi(builder.Configuration);

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
