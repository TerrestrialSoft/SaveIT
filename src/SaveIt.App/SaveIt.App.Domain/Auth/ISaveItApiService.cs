using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;

public interface ISaveItApiService
{
    Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<OAuthCompleteTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<Result<string>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}