using FluentResults;
using SaveIt.Server.UI.Models;

namespace SaveIt.Server.UI.Services.Auth;

/// <summary>
/// Represents a service for Google OAuth authentication.
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Registers an authorization request.
    /// </summary>
    /// <param name="requestId">Request id</param>
    /// <param name="serverUrl">Server url used for redirection</param>
    /// <returns>Authorization model containing Authorization Url</returns>
    AuthorizationModel RegisterAuthorizationRequest(Guid requestId, string serverUrl);

    /// <summary>
    /// Gets the tokens from the Google OAuth service and saves them.
    /// </summary>
    /// <param name="authorizationCode">Authorization code parameter value</param>
    /// <param name="urlEncodedState">Encoded state parameter</param>
    /// <param name="serverUrl">Server Url</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of obtaining and saving tokens</returns>
    Task<Result> ObtainAndSaveTokens(string authorizationCode, string urlEncodedState, string serverUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the tokens from the storage.
    /// </summary>
    /// <param name="requestId">Stored request key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retrieves tokens from Google API</returns>
    Task<Result<OAuthCompleteTokenResponseModel>> RetrieveTokensAsync(Guid requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the access token using the refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of obtaining Access Token</returns>
    Task<Result<OAuthAccessTokenResponseModel>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
