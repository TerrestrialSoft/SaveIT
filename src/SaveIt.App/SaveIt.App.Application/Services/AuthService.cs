using SaveIt.App.Domain.Auth;

namespace SaveIt.App.Application.Services;
public class AuthService(IAuthClientService saveItClient) : IAuthService
{
    public async Task<Uri> GetAuthorizationUrlAsync()
    {
        return await saveItClient.GetAuthorizationUrlAsync(Guid.NewGuid());
    }
}
