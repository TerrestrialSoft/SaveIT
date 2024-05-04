using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using FluentResults;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Enums;

namespace SaveIt.App.Application.Services;
public class AuthService(ISaveItApiService _saveItClient, IStorageAccountRepository _accountRepository,
    IAccountSecretsRepository _secretsService, IExternalStorageService _externalStorage,
    ISaveItApiTokenRetrievalService _saveItTokenClient)
    : IAuthService
{
    public async Task<Result<Uri>> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        try
        {
            Uri url = await _saveItClient.GetAuthorizationUrlAsync(requestId, cancellationToken);
            return Result.Ok(url);
        }
        catch (Exception)
        {
            return Result.Fail("Error occured during contacting the server");
        }
    }

    public async Task<Result> WaitForAuthorizationAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var tokenResult = await GetTokenAsync(requestId, cancellationToken);

        if (tokenResult.IsFailed)
        {
            return tokenResult.ToResult();
        }

        var emailResult = await GetUserEmailAsync(tokenResult.Value.AccessToken, cancellationToken);

        if (emailResult.IsFailed)
        {
            return emailResult.ToResult();
        }

        var accounts = await _accountRepository.GetAccountsWithEmailAsync(emailResult.Value);
        var account = accounts.FirstOrDefault(x => x.Type == StorageAccountType.Google);

        Guid accountId = account is null
            ? Guid.NewGuid()
            : account.Id;

        var result = await _secretsService.StoreTokensAsync(accountId, tokenResult.Value.AccessToken,
             tokenResult.Value.RefreshToken);

        if (result.IsFailed)
        {
            return result;
        }

        if(account is { IsAuthorized: false})
        {
            account.IsAuthorized = true;
            await _accountRepository.UpdateAsync(account);
            return Result.Ok();
        }
        else if (account is not null)
        {
            return Result.Ok();
        }

        account = new()
        {
            Id = accountId,
            Email = emailResult.Value,
            Type = StorageAccountType.Google,
            IsAuthorized = true,
        };

        await _accountRepository.CreateAsync(account);

        return Result.Ok();
    }

    private async Task<Result<string>> GetUserEmailAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var result = await _externalStorage.GetProfileEmailAsync(accessToken, cancellationToken);
        return result;
    }

    private async Task<Result<OAuthCompleteTokenModel>> GetTokenAsync(Guid requestId,
        CancellationToken cancellationToken = default)
    {
        OAuthCompleteTokenModel? tokenModel;
        try
        {
            tokenModel = await _saveItTokenClient.GetTokenAsync(requestId, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or ArgumentNullException)
        {
            return Result.Fail("Error occured during contacting the server");
        }

        return tokenModel is not null
            ? Result.Ok(tokenModel)
            : Result.Fail("Invalid token");
    }
}
