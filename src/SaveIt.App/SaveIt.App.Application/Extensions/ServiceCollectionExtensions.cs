using Microsoft.Extensions.DependencyInjection;
using SaveIt.App.Application.Services;
using SaveIt.App.Domain.Auth;

namespace SaveIt.App.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services.AddScoped<IAuthService, AuthService>();
}
