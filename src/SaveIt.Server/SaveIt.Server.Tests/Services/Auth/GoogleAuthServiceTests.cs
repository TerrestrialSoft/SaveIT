using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SaveIt.Server.Tests.Helpers;
using SaveIt.Server.UI.Models;
using SaveIt.Server.UI.Options;
using SaveIt.Server.UI.Services;
using SaveIt.Server.UI.Services.Auth;
using System.Net;
using System.Text.Json;

namespace SaveIt.Server.Tests.Services.Auth;
public class GoogleAuthServiceTests
{
    private const string ServerUrl = "some-server-url";
    private const string SecurityToken = "some-state";

    private static (IOptions<GoogleClientOptions>, ILogger<GoogleAuthService>) ArrangeDependencies()
    {
        var googleConfigOptions = Options.Create(new GoogleClientOptions
        {
            ClientId = "clientId",
            ClientSecret = "clientSecret",
            TokenUrl = "tokenUrl",
            OAuthUrl = "oauthUrl",
            LocalRedirectUrl = "localRedirectUrl"
        });
        var logger = Substitute.For<ILogger<GoogleAuthService>>();

        return (googleConfigOptions, logger);
    }

    private static HttpClient ArrangeHttpClient(HttpStatusCode statusCode = HttpStatusCode.OK, object? responseContent = null)
    {
        var handler = new MockHttpMessageHandler(statusCode, responseContent);
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://some-url.com/")
        };

        return client;
    }

    [Fact]
    public void RegisterAuthorizationRequest_RequestIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        void act() => service.RegisterAuthorizationRequest(Guid.Empty, ServerUrl);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void RegisterAuthorizationRequest_ServerUrlIsNullOrEmpty_ThrowsArgumentException()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        void act() => service.RegisterAuthorizationRequest(Guid.NewGuid(), string.Empty);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void RegisterAuthorizationRequest_ServerUrlIsPresent_ReturnsAuthorizationModel()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        oAuthProvider.GetStateParameter()
            .Returns(SecurityToken);
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();
        var requestId = Guid.NewGuid();
        var storedRequest = new StoredRequest(new StateModel(SecurityToken, requestId));

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = service.RegisterAuthorizationRequest(requestId, ServerUrl);

        // Assert
        tokenStorage.Received().SetToken(requestId, storedRequest);
        Assert.NotNull(result);
        Assert.Equal(SecurityToken, result.State.SecurityToken);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_StateIsNullOrEmpty_ReturnsFailedResult()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", string.Empty, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_StateRequestIdIsNotPresent_ReturnsFailedResult()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var state = new StateModel(SecurityToken, Guid.Empty);
        var stateString = JsonSerializer.Serialize(state);

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", stateString, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_SecurityTokenMissmatch_ReturnsFailedResult()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var storedState = new StateModel(SecurityToken, requestId);

        var (googleConfigOptions, logger) = ArrangeDependencies();
        var client = ArrangeHttpClient();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();
        tokenStorage.TryGetToken(requestId, out Arg.Any<StoredRequest>()!)
            .Returns(x =>
            {
                x[1] = new StoredRequest(storedState);
                return true;
            });

        var invalidSecurityToken = "invalid-security-token";
        var state = new StateModel(invalidSecurityToken, requestId);
        var stateString = JsonSerializer.Serialize(state);

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", stateString, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_StoredRequestNotPresent_ReturnsFailedResult()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        var (googleConfigOptions, logger) = ArrangeDependencies();
        var client = ArrangeHttpClient();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();
        tokenStorage.TryGetToken(requestId, out Arg.Any<StoredRequest>()!)
            .Returns(false);

        var state = new StateModel(SecurityToken, requestId);
        var stateString = JsonSerializer.Serialize(state);

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", stateString, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_ObtainingWasNotSuccessful_ReturnsFailedResult()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var state = new StateModel(SecurityToken, requestId);

        var (googleConfigOptions, logger) = ArrangeDependencies();
        var client = ArrangeHttpClient(HttpStatusCode.BadRequest);
        
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();
        tokenStorage.TryGetToken(requestId, out Arg.Any<StoredRequest>()!)
            .Returns(x =>
            {
                x[1] = new StoredRequest(state);
                return true;
            });

        var stateString = JsonSerializer.Serialize(state);

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", stateString, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ObtainAndSaveTokens_ObtainingTokenWasSuccessful_StoreTokens()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var state = new StateModel(SecurityToken, requestId);
        var tokenResponseModel = new OAuthCompleteTokenResponseModel("access-token", "refresh-token",
           "scope", "token-type", 3600);

        var (googleConfigOptions, logger) = ArrangeDependencies();
        var client = ArrangeHttpClient(responseContent: tokenResponseModel);

        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();
        tokenStorage.TryGetToken(requestId, out Arg.Any<StoredRequest>()!)
            .Returns(x =>
            {
                x[1] = new StoredRequest(state);
                return true;
            });

        var stateString = JsonSerializer.Serialize(state);

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        // Act
        var result = await service.ObtainAndSaveTokens("code", stateString, ServerUrl, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        tokenStorage.Received().SetToken(requestId,
            Arg.Is<StoredRequest>(x => x.Token!.AccessToken == tokenResponseModel.AccessToken
                && x.Token.RefreshToken == tokenResponseModel.RefreshToken));
    }

    [Fact]
    public async Task RetrieveTokens_ReturnTokens()
    {
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var client = ArrangeHttpClient();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        var requestId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Act
        await service.RetrieveTokensAsync(requestId, cancellationToken);

        // Assert
        await tokenStorage.Received().WaitForTokenAsync(requestId, cancellationToken);
    }

    [Fact]
    public async Task RefreshAccessToken_UnsuccessfulResponse_ReturnsFailedResult()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var client = ArrangeHttpClient(HttpStatusCode.BadRequest);
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        var refreshToken = "refresh-token";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await service.RefreshAccessTokenAsync(refreshToken, cancellationToken);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task RefreshAccessToken_SuccessfulResponse_ReturnsToken()
    {
        // Arrange
        var (googleConfigOptions, logger) = ArrangeDependencies();
        var responseModel = new OAuthAccessTokenResponseModel("access-token", "token-type", "tokenType",
            3600);
        var client = ArrangeHttpClient(responseContent: responseModel);
        var oAuthProvider = Substitute.For<IOAuthStateProvider>();
        var tokenStorage = Substitute.For<ITokenStorage>();

        var service = new GoogleAuthService(oAuthProvider, client, googleConfigOptions, tokenStorage, logger);

        var refreshToken = "refresh-token";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await service.RefreshAccessTokenAsync(refreshToken, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(responseModel.AccessToken, result.Value.AccessToken);
    }
}
