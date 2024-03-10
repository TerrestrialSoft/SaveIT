using FluentResults;
using SaveIt.Server.UI.Models;

namespace SaveIt.Server.UI.Services;

public interface ITokenStorage
{
    void SetToken(Guid key, StoredRequest value);
    bool TryGetToken(Guid key, out StoredRequest? value);
    Task<Result<OAuthTokenModel>> WaitForToken(Guid key, CancellationToken cancellationToken);
}
