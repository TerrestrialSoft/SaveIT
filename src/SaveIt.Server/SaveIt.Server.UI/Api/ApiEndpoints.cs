using SaveIt.Server.UI.Models;
using SaveIt.Server.UI.Results;
using SaveIt.Server.UI.Services.Auth;
using System.Net;
using System.Web;

namespace SaveIt.Server.UI.Api;

public static class ApiEndpoints
{
    private const string _successAuth = "/auth/success";
    private const string _failedAuth = "/auth/failed?error={0}";

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("api/auth");

        apiGroup.MapPost("google", (RequestModel model, HttpContext context, IGoogleAuthService authService) =>
        {
            var serverUrl = GetServerUrl(context.Request);
            var authorization = authService.RegisterAuthorizationRequest(model.RequestId, serverUrl);

            return Microsoft.AspNetCore.Http.Results.Ok(new AuthorizationUrlResponseModel(authorization.Uri.ToString()));
        });

        apiGroup.MapGet("google/callback", async (string state, string? code, string? error, HttpContext context,
            CancellationToken cancellationToken, IGoogleAuthService authService, ILogger<GoogleAuthService> _logger) =>
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                _logger.LogInformation("Failed to authenticate with Google: {Error}", error);
                return Microsoft.AspNetCore.Http.Results.Redirect(string.Format(_failedAuth, error));
            }

            var serverUrl = GetServerUrl(context.Request);
            var result = await authService.GetTokensAsync(code, state, serverUrl, cancellationToken);

            var path = _successAuth;

            if (result.IsFailed)
            {
                var errors = HttpUtility.UrlEncode(result.Errors.ToString());
                path = string.Format(_failedAuth, errors);
            }

            return Microsoft.AspNetCore.Http.Results.Redirect(path);
        });

        apiGroup.MapPost("retrieve", async (RequestModel model, CancellationToken token,
            IGoogleAuthService authService) =>
        {
            var result = await authService.RetrieveTokensAsync(model.RequestId, token);

            return result.IsSuccess
                ? Microsoft.AspNetCore.Http.Results.Ok(result.Value)
                : Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.RequestTimeout);
        });

        apiGroup.MapPost("refresh", async (RefreshTokenRequestModel model, CancellationToken token,
                       IGoogleAuthService authService) =>
        {
            var result = await authService.RefreshAccessTokenAsync(model.RefreshToken, token);

            return result.ToAspResult();
        });
    }

    private static string GetServerUrl(HttpRequest request)
        => $"{request.Scheme}://{request.Host}";
}
