using FluentResults;
using SaveIt.Server.UI.Models;

namespace SaveIt.Server.UI.Services.Auth;

public interface IGoogleAuthService
{
    AuthorizationModel RegisterAuthorizationRequest(Guid requestId);
    Task<Result> GetTokensAsync(string code, string state, CancellationToken cancellationToken);
    Task<Result<OAuthTokenModel>> RetrieveTokensAsync(Guid requestId, CancellationToken cancellationToken);
}
