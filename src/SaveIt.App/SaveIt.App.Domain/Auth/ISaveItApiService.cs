using FluentResults;

namespace SaveIt.App.Domain.Auth;

public interface ISaveItApiService
{
    Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<Result<string>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}