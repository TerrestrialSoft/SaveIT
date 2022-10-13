using SaveIT.App.Context;
using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;
using SaveIT.Core.Services;
using SaveIT.Core.Storage;
using SaveIT.Storage;

namespace SaveIT.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services.AddScoped<IStorage<GameProfile>, SQLiteStorage<GameProfile>>()
			.AddScoped<IStorage<OAuthToken>, SQLiteStorage<OAuthToken>>()
            .AddScoped<IGameProfileService, GameProfileService>()
            .AddScoped<IRepository<GameProfile>, GameProfileRepository>()
            .AddScoped<IRepository<OAuthToken>, OAuthTokenRepository>()
			.AddScoped<ICloudStorage, GoogleDriveStorage>()
			.AddScoped<IExternalStorageService, ExternalStorageService>()
			.AddScoped<ITokenService, TokenService>();

	public static IServiceCollection AddCurrentContext(this IServiceCollection services)
		=> services.AddSingleton<CurrentContext>();

	public static IServiceCollection AddExternalServices(this IServiceCollection services)
		=> services.AddHttpClient();
}
