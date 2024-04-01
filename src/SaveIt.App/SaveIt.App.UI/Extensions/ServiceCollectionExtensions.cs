using Microsoft.Extensions.Configuration;
using SaveIt.App.Domain;
using SaveIt.App.Persistence.Extensions;
using System.Reflection;

namespace SaveIt.App.UI.Extensions;
internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddUi(this IServiceCollection services, IConfiguration config)
        => services.AddPersistence()
            .AddInfrastructure(config)
            .AddServices();

    private static IServiceCollection AddServices(this IServiceCollection services)
        => services.AddScoped<IApplicationContext, ApplicationContext>();

    internal static void AddAppJsonConfiguration(this IConfigurationManager configurationManager)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var name = Assembly.GetExecutingAssembly().GetName().Name + ".appsettings{0}.json";


        using var appSettingsStream = executingAssembly.GetManifestResourceStream(string.Format(name, ""));
#if DEBUG
        using var appSettingsEnvStream = executingAssembly.GetManifestResourceStream(string.Format(name, ".Development"));
#else
        using var appSettingsEnvStream = executingAssembly.GetManifestResourceStream(string.Format(name, ".Production"));
#endif
        var config = new ConfigurationBuilder()
            .AddJsonStream(appSettingsStream!)
            .AddJsonStream(appSettingsEnvStream!)
            .Build();

        configurationManager.AddConfiguration(config);
    }
}
