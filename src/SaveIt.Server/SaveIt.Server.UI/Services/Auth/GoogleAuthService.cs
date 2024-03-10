using FluentResults;
using Flurl;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SaveIt.Server.UI.Models;
using SaveIt.Server.UI.Options;
using System.Web;

namespace SaveIt.Server.UI.Services.Auth;

public class GoogleAuthService(IOAuthStateProvider oAuthProvider,
    HttpClient client,
    IOptions<GoogleClientOptions> googleConfigOptions,
    ITokenStorage tokenStorage,
    ILogger<GoogleAuthService> logger)
    : IGoogleAuthService
{
    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly ILogger<GoogleAuthService> _logger = logger;
    private readonly IOAuthStateProvider _oAuthProvider = oAuthProvider;
    private readonly HttpClient _client = client;
    private readonly GoogleClientOptions _clientConfig = googleConfigOptions.Value;

    private const string _redirectUri = "https://localhost:44375/auth/google/callback";
    private const string _baseUrl = "https://accounts.google.com/o/oauth2/v2/auth";

    public AuthorizationModel RegisterAuthorizationRequest(Guid requestId)
    {
        var token = _oAuthProvider.GetSecurityToken();

        var state = new StateModel(token, requestId);
        
        _tokenStorage.SetToken(requestId, new StoredRequest(state));

        var encodedState = HttpUtility.UrlEncode(JsonConvert.SerializeObject(state));

        var builder = new UriBuilder(_baseUrl);
        builder.Query = builder.Query.SetQueryParams(new Dictionary<string, string>()
        {
            { "client_id", _clientConfig.ClientId },
            { "state", encodedState },
            { "redirect_uri", _redirectUri },
            { "scope", "https://www.googleapis.com/auth/drive" },
            { "response_type", "code" },
            { "access_type", "offline" },
        });


        return new(builder.Uri, state);
    }

    public async Task<Result> GetTokensAsync(string code, string urlEncodedState, CancellationToken cancellationToken)
    {
        var decodedState = HttpUtility.UrlDecode(urlEncodedState);
        var state = JsonConvert.DeserializeObject<StateModel>(decodedState);

        if (state is null || state.RequestId == default)
        {
            return Result.Fail("Provided data are invalid.");
        }

        Result validationResult = ValidateSecurityToken(state.RequestId, state.SecurityToken);

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var content = new Dictionary<string, string>()
        {
            { "client_id", _clientConfig.ClientId },
            { "client_secret", _clientConfig.ClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", _redirectUri },
        };

        var result = await _client.PostAsync("", new FormUrlEncodedContent(content), cancellationToken); // Add retry mechanisms

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError("Error occured during communication with identity provider. {ReasonPhrase}", result.ReasonPhrase);
            return Result.Fail($"Identity Provider communication error");
        }

        var token = await result.Content.ReadFromJsonAsync<OAuthTokenModel>(cancellationToken: cancellationToken);

        if (token is null)
        {
            _logger.LogError("Error occured during deserialization of Identity provider response. {token}", token);
            return Result.Fail("Identity Provider communication error.");
        }

        _tokenStorage.SetToken(state.RequestId, new StoredRequest(state, token));
        return Result.Ok();
    }

    private Result ValidateSecurityToken(Guid requestId, string? securityToken)
    {
        if (!_tokenStorage.TryGetToken(requestId, out var value)
            || value!.State.SecurityToken != securityToken)
        {
            _logger.LogWarning("State parameter missmatch");
            return Result.Fail($"Error occured during communication with identity provider.");
        }

        return Result.Ok();
    }

    public async Task<Result<OAuthTokenModel>> RetrieveTokensAsync(Guid requestId, CancellationToken cancellationToken)
        => await _tokenStorage.WaitForToken(requestId, cancellationToken);
}
