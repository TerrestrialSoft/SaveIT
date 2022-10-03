using SaveIT.Api.Services;

namespace SaveIT.Web.Extensions;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddCustomServices(this IServiceCollection services)
		=> services.AddScoped<IOAuthService, OAuthService>()
			.AddHttpClient()
			.AddMemoryCache();
}
