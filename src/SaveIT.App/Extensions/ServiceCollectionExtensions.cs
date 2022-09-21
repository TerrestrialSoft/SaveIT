using Microsoft.Extensions.Configuration;
using SaveIT.App.Context;
using SaveIT.CloudStorage.Options;
using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;
using SaveIT.Core.Services;
using SaveIT.Core.Storage;
using SaveIT.Storage;
using System.Reflection;

namespace SaveIT.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services.AddScoped<IStorage<GameProfile>, SQLiteStorage<GameProfile>>()
            .AddScoped<IGameProfileService, GameProfileService>()
            .AddScoped<IRepository<GameProfile>, GameProfileRepository>()
			.AddScoped<ICloudStorage, GoogleDriveStorage>();

	public static IServiceCollection AddCurrentContext(this IServiceCollection services)
		=> services.AddSingleton<CurrentContext>();

	public static IServiceCollection AddConfigurationProvider(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("SaveIT.App.appsettings.json");

		var config = new ConfigurationBuilder()
			.AddJsonStream(stream)
			.Build();

		configurationManager.AddConfiguration(config);
		services.Configure<GoogleDriveOAuthOptions>(x =>
			configurationManager.GetSection(GoogleDriveOAuthOptions.Position)
			.Bind(x));

		return services;
	}

}
