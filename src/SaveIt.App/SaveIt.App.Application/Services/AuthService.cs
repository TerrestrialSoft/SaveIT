using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using Polly;
using FluentResults;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Enums;
using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Application.Services;
public class AuthService(ISaveItApiService _saveItClient, IStorageAccountRepository _accountRepository,
    IAccountSecretsService _secretsService, IExternalStorageService _externalStorage) : IAuthService
{

    public async Task<Result<Uri>> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken)
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

    public async Task<Result> WaitForAuthorizationAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var tokenResult = await GetTokenAsync(requestId, cancellationToken);

        if (tokenResult.IsFailed)
        {
            return tokenResult.ToResult();
        }

        Guid accountId = Guid.NewGuid();

        var result = await _secretsService.StoreTokensAsync(accountId, tokenResult.Value.AccessToken,
            tokenResult.Value.RefreshToken);

        if(result.IsFailed)
        {
            return result;
        }

        var emailResult = await GetUserEmailAsync(tokenResult.Value.AccessToken);

        if (emailResult.IsFailed)
        {
            return emailResult.ToResult();
        }

        var accounts = await _accountRepository.GetAccountsWithEmailAsync(emailResult.Value);

        if(accounts.Any(x => x.Type == StorageAccountType.Google))
        {
            return Result.Fail("Account already exists");
        }

        StorageAccount account = new()
        {
            Id = accountId,
            Email = emailResult.Value,
            Type = StorageAccountType.Google
        };

        await _accountRepository.AddAccountAsync(account);

        return Result.Ok();
    }

    private async Task<Result<string>> GetUserEmailAsync(string accessToken)
    {
        try
        {
            var email = await _externalStorage.GetProfileEmailAsync(accessToken);
            return Result.Ok(email);
        }
        catch (Exception)
        {
            return Result.Fail("Error occured during communication with the external server");
        }
    }

    private async Task<Result<OAuthCompleteTokenModel>> GetTokenAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var retryPipeline = new ResiliencePipelineBuilder<OAuthCompleteTokenModel?>()
        .AddRetry(new()
        {
            MaxRetryAttempts = 40,
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.FromSeconds(15),
            ShouldHandle = new PredicateBuilder<OAuthCompleteTokenModel?>()
                .Handle<HttpRequestException>(),
        })
        .Build();

        OAuthCompleteTokenModel? token = null;

        try
        {
            token = await retryPipeline.ExecuteAsync(async token => await _saveItClient.GetTokenAsync(requestId, token),
                cancellationToken);
        }
        catch (Exception)
        {
            return Result.Fail("Error occured during contacting the server");
        }

        return token is not null
            ? Result.Ok(token)
            : Result.Fail("Invalid token");
    }
}
