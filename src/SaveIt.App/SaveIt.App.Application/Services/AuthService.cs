using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using Polly;
using FluentResults;

namespace SaveIt.App.Application.Services;
public class AuthService(IAuthClientService saveItClient) : IAuthService
{
    public async Task<Result<Uri>> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken)
    {
        try
        {
            Uri url = await saveItClient.GetAuthorizationUrlAsync(requestId, cancellationToken);
            return Result.Ok(url);
        }
        catch (Exception ex)
        {
            return Result.Fail("Error occured during contacting the server");
        }
    }

    public async Task<Result> WaitForAuthorizationAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var retryPipeline = new ResiliencePipelineBuilder<OAuthTokenModel?>()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<OAuthTokenModel?>()
                    .Handle<HttpRequestException>(),
            })
            .Build();

        OAuthTokenModel? token = null;

        try
        {
            token = await retryPipeline.ExecuteAsync(async token => await saveItClient.GetTokenAsync(requestId, token),
                cancellationToken);
        }
        catch(Exception)
        {
            return Result.Fail("Error occured during contacting the server");
        }
       

        if (token is null)
        {
            return Result.Fail("Invalid token");
        }




        return Result.Ok();
    }
}
