using FluentResults;

namespace SaveIt.App.Domain.Repositories;
public interface IAccountSecretsService
{
    Task<Result> StoreTokensAsync(Guid accountId, string accessToken, string refreshToken);
    Task<string?> GetAccessTokenAsync(Guid accountId);
    Task<string?> GetRefreshTokenAsync(Guid accountId);
}
