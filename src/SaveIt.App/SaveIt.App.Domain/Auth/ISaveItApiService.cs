using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;

public interface ISaveItApiService
{
    Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken);
    Task<OAuthTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken);
}