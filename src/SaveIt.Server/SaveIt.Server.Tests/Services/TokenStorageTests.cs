using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SaveIt.Server.UI.Models;
using SaveIt.Server.UI.Options;
using SaveIt.Server.UI.Services;

namespace SaveIt.Server.Tests.Services;
public class TokenStorageTests
{
    private static (IMemoryCache, IOptions<TokenCachingOptions>, ILogger<TokenStorage>) ArrangeDependencies(
        int entryLifeTimeSeconds = 10, int tokenRetrieveSeconds = 5, int retrieveDelaySeconds = 1)
    {
        var cache = Create.MockedMemoryCache();
        var tokenCachingOptions = Options.Create(new TokenCachingOptions
        {
            CacheEntryLifeTimeSeconds = entryLifeTimeSeconds,
            MaxTokenRetrieveTimeSeconds = tokenRetrieveSeconds,
            TokenRetrieveDelaySeconds = retrieveDelaySeconds
        });
        var logger = Substitute.For<ILogger<TokenStorage>>();
        return (cache, tokenCachingOptions, logger);
    }

    private static StoredRequest CreateStoredRequest(Guid requestId)
        => new(new StateModel("security-token", requestId))
        {
            Token = new OAuthCompleteTokenResponseModel("accesToken", "refreshToken", "scope",
                "tokenType", 5)
        };

    [Fact]
    public void SetToken_SetsToken()
    {
        // Arrange
        var (cache, tokenCachingOptions, logger) = ArrangeDependencies();
        var tokenStorage = new TokenStorage(cache, tokenCachingOptions, logger);
        var requestId = Guid.NewGuid();
        var storedRequest = CreateStoredRequest(requestId);

        // Act
        tokenStorage.SetToken(requestId, storedRequest);

        // Assert
        Assert.True(cache.TryGetValue(TokenStorage.GetCacheKey(requestId), out var result));
        Assert.Equal(storedRequest, result);
    }

    [Fact]
    public void TryGetToken_TokenExists_ReturnsTrue()
    {
        // Arrange
        var (cache, tokenCachingOptions, logger) = ArrangeDependencies();
        var tokenStorage = new TokenStorage(cache, tokenCachingOptions, logger);
        var requestId = Guid.NewGuid();
        var storedRequest = CreateStoredRequest(requestId);
        cache.Set(TokenStorage.GetCacheKey(requestId), storedRequest);

        // Act
        var result = tokenStorage.TryGetToken(requestId, out var resultStoredRequest);

        // Assert
        Assert.True(result);
        Assert.Equal(storedRequest, resultStoredRequest);
    }

    [Fact]
    public void TryGetToken_TokenDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var (cache, tokenCachingOptions, logger) = ArrangeDependencies();
        var tokenStorage = new TokenStorage(cache, tokenCachingOptions, logger);
        var requestId = Guid.NewGuid();

        // Act
        var result = tokenStorage.TryGetToken(requestId, out var resultStoredRequest);

        // Assert
        Assert.False(result);
        Assert.Null(resultStoredRequest);
    }

    [Fact]
    public async Task WaitForTokenAsync_TokenExists_ReturnsToken()
    {
        // Arrange
        var (cache, tokenCachingOptions, logger) = ArrangeDependencies();
        var tokenStorage = new TokenStorage(cache, tokenCachingOptions, logger);
        var requestId = Guid.NewGuid();
        var storedRequest = CreateStoredRequest(requestId);
        cache.Set(TokenStorage.GetCacheKey(requestId), storedRequest);

        // Act
        var result = await tokenStorage.WaitForTokenAsync(requestId, CancellationToken.None);

        // Assert
        Assert.Equal(storedRequest.Token, result.Value);
    }

    [Fact]
    public async Task WaitForTokenAsync_TokenNotExistsAfterMaxTime_ReturnsFailedResult()
    {
        // Arrange
        var (cache, tokenCachingOptions, logger) = ArrangeDependencies(tokenRetrieveSeconds: 1);
        var tokenStorage = new TokenStorage(cache, tokenCachingOptions, logger);
        var requestId = Guid.NewGuid();

        // Act
        var result = await tokenStorage.WaitForTokenAsync(requestId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }
}
