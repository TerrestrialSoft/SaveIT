using FluentResults;

namespace SaveIt.App.Domain.Repositories;
public interface IAccountSecretsRepository
{
    Task<Result> StoreTokensAsync(Guid accountId, string accessToken, string refreshToken);
    Task<string?> GetAccessTokenAsync(Guid accountId);
    Task<string?> GetRefreshTokenAsync(Guid accountId);
    Task<Result> StoreAccessTokenAsync(Guid accountId, string accessToken);
    void ClearAccount(Guid accountId);
}
