using Microsoft.AspNetCore.Components.WebView.Maui;
using SaveIT.Core.Services;

namespace SaveIT.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddScoped<IGameProfileService, GameProfileService>();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		#endif

		return builder.Build();
	}
}
