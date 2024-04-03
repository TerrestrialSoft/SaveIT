using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using SaveIt.App.Application.Extensions;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Infrastructure.Api;

namespace SaveIt.App.Persistence.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddApplication()
            .AddTypedHttpClients(configuration);

    private static IServiceCollection AddTypedHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IAuthClientService, SaveItClient>(client =>
        {
            Uri uri = new Uri(configuration["SaveItApi:Url"]);
            client.BaseAddress = uri;
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(_ =>
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(x => x.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        });

        return services;
    }
}
