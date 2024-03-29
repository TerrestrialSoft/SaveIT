using Microsoft.Extensions.DependencyInjection;
using SaveIt.App.Domain;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Persistence.Repositories;

namespace SaveIt.App.Persistence.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
        => services.AddScoped<IGameRepository, GameRepository>()
            .AddScoped<IGameSaveRepository, GameSaveRepository>()
            .AddSingleton<IDatabaseHandler, DatabaseHandler>();
}
