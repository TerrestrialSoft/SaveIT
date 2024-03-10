using SaveIt.Server.UI.Options;
using SaveIt.Server.UI.Services;
using SaveIt.Server.UI.Services.Auth;

namespace SaveIt.Server.UI.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services.AddCustomOptions(configuration)
            .AddExternalServices()
            .AddApplicationServices()
            .AddHttpClients(configuration);

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services.AddScoped<IOAuthStateProvider, OAuthStateProvider>()
            .AddScoped<IGoogleAuthService, GoogleAuthService>()
            .AddScoped<ITokenStorage, TokenStorage>();

    private static IServiceCollection AddExternalServices(this IServiceCollection services)
        => services.AddMemoryCache();

    private static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        => services.Configure<GoogleClientOptions>(configuration.GetSection(GoogleClientOptions.Path));

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration.GetSection(GoogleClientOptions.Path).GetValue<string>(nameof(GoogleClientOptions.TokenUrl))!;

        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>(client => client.BaseAddress = new Uri(url));

        return services;
    }
}
