using FluentResults;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.UI.Service.Local;
internal class AccountSecretsService : IAccountSecretsService
{
    private const string AccessTokenKey = "{0}:AccessToken";
    private const string RefreshToken = "{0}:RefreshToken";

    private static async Task<string?> GetValue(string template, Guid accountId)
        => await SecureStorage.Default.GetAsync(string.Format(template, accountId));

    private static async Task SetValue(string template, Guid accountId, string value)
        => await SecureStorage.Default.SetAsync(string.Format(template, accountId), value);

    public Task<string?> GetAccessToken(Guid accountId)
        => GetValue(AccessTokenKey, accountId);

    public Task<string?> GetRefreshToken(Guid accountId)
        => GetValue(RefreshToken, accountId);

    public async Task<Result> StoreTokensAsync(Guid accountId, string accessToken, string refreshToken)
    {
        try
        {
            await SetValue(AccessTokenKey, accountId, accessToken);
            await SetValue(RefreshToken, accountId, refreshToken);
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to store data to storage");
        }
        
    }
}
