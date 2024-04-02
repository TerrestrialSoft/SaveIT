using FluentResults;
using SaveIt.Server.UI.Models;

namespace SaveIt.Server.UI.Services.Auth;

public interface IGoogleAuthService
{
    AuthorizationModel RegisterAuthorizationRequest(Guid requestId, string serverUrl);
    Task<Result> GetTokensAsync(string code, string urlEncodedState, string serverUrl, CancellationToken cancellationToken);
    Task<Result<OAuthTokenModel>> RetrieveTokensAsync(Guid requestId, CancellationToken cancellationToken);
}
