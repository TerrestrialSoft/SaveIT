using FluentResults;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.UI.Service.Local;
internal class AccountSecretsService : IAccountSecretsService
{
    private const string AccessTokenKey = "{0}:AccessToken";
    private const string RefreshToken = "{0}:RefreshToken";

    private static async Task<string?> GetValueAsync(string template, Guid accountId)
        => await SecureStorage.Default.GetAsync(string.Format(template, accountId));

    private static async Task SetValueAsync(string template, Guid accountId, string value)
        => await SecureStorage.Default.SetAsync(string.Format(template, accountId), value);

    public void ClearAccount(Guid accountId)
    {
        SecureStorage.Default.Remove(string.Format(AccessTokenKey, accountId));
        SecureStorage.Default.Remove(string.Format(RefreshToken, accountId));
    }

    public Task<string?> GetAccessTokenAsync(Guid accountId)
        => GetValueAsync(AccessTokenKey, accountId);

    public Task<string?> GetRefreshTokenAsync(Guid accountId)
        => GetValueAsync(RefreshToken, accountId);

    public async Task<Result> StoreAccessTokenAsync(Guid accountId, string accessToken)
    {
        try
        {
            await SetValueAsync(AccessTokenKey, accountId, accessToken);
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to store data to storage");
        }
    }

    public async Task<Result> StoreTokensAsync(Guid accountId, string accessToken, string refreshToken)
    {
        try
        {
            await SetValueAsync(AccessTokenKey, accountId, accessToken);
            await SetValueAsync(RefreshToken, accountId, refreshToken);
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to store data to storage");
        }
    }
}
