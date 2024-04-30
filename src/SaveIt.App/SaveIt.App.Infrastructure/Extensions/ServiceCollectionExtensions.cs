using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SaveIt.App.Application.Extensions;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Infrastructure.Api;
using SaveIt.App.Infrastructure.Options;

namespace SaveIt.App.Persistence.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services.AddApplication()
            .AddOptions(configuration)
            .AddTypedHttpClients();

    private static IServiceCollection AddTypedHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ISaveItApiService, SaveItApiService>((serviceProvider, client) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<SaveItApiOptions>>();
            client.BaseAddress = new Uri(apiOptions.Value.Url);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(_ => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(x => !x.IsSuccessStatusCode && x.StatusCode != System.Net.HttpStatusCode.Unauthorized
                    && x.StatusCode != System.Net.HttpStatusCode.Forbidden)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddHttpClient<IExternalStorageService, GoogleApiService>((serviceProvider, client) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<GoogleApiOptions>>();
            client.BaseAddress = new Uri(apiOptions.Value.DriveUrl);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(_ => HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddHttpClient<GoogleApiUploadService>((serviceProvider, client) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<GoogleApiOptions>>();
            client.BaseAddress = new Uri(apiOptions.Value.DriveUploadUrl);
        })
       .SetHandlerLifetime(TimeSpan.FromMinutes(5))
       .AddPolicyHandler(_ => HttpPolicyExtensions
               .HandleTransientHttpError()
               .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        => services.Configure<SaveItApiOptions>(configuration.GetSection(SaveItApiOptions.Path))
            .Configure<GoogleApiOptions>(configuration.GetSection(GoogleApiOptions.Path));
}
