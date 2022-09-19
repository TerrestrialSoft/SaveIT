using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;
using SaveIT.Core.Services;
using SaveIT.Core.Storage;

namespace SaveIT.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services.AddScoped<IStorage<GameProfile>, SQLiteStorage<GameProfile>>()
            .AddScoped<IGameProfileService, GameProfileService>()
            .AddScoped<IRepository<GameProfile>, GameProfileRepository>();

}
