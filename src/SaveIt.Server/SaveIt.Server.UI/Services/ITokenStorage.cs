using FluentResults;
using SaveIt.Server.UI.Models;

namespace SaveIt.Server.UI.Services;

/// <summary>
/// Represents a local in-memory storage for OAuth tokens.
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Sets the token in the storage.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="value">Storage value</param>
    void SetToken(Guid key, StoredRequest value);

    /// <summary>
    /// Tries to get the token from the storage.
    /// </summary>
    /// <param name="key">Storge key</param>
    /// <param name="value">Storage value</param>
    /// <returns>true if key was found, false otherwise</returns>
    bool TryGetToken(Guid key, out StoredRequest? value);

    /// <summary>
    /// Waits for the token to be set in the storage.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<Result<OAuthCompleteTokenResponseModel>> WaitForTokenAsync(Guid key, CancellationToken cancellationToken = default);
}
