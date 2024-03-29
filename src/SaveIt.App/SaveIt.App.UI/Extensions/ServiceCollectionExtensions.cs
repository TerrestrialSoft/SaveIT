using SaveIt.App.Domain;

namespace SaveIt.App.UI.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUI(this IServiceCollection services)
        => services.AddScoped<IApplicationContext, ApplicationContext>();
}
