using SaveIt.Server.UI.Services.Auth;
using System.Web;

namespace SaveIt.Server.UI.Api;

public static class ApiEndpoints
{
    private const string _successAuth = "/auth/success";
    private const string _failedAuth = "/auth/failed?error={0}";

    public static void AddApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("api/auth")
            .DisableAntiforgery();

        apiGroup.MapGet("google", (Guid requestId, IGoogleAuthService authService) =>
        {
            var authorization = authService.RegisterAuthorizationRequest(requestId);

            return Results.Ok(authorization.Uri.ToString());
        });

        apiGroup.MapGet("google/callback", async (string state, string? code, string? error, CancellationToken cancellationToken,
            IGoogleAuthService authService, ILogger<GoogleAuthService> _logger) =>
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                _logger.LogInformation("Failed to authenticate with Google: {Error}", error);
                return Results.Redirect(string.Format(_failedAuth, error));
            }

            string path = _successAuth;
            var result = await authService.GetTokensAsync(code, state, cancellationToken);

            if (result.IsFailed)
            {
                var errors = HttpUtility.UrlEncode(result.Errors.ToString());
                path = string.Format(_failedAuth, errors);
            }

            return Results.Redirect(path);
        });

        apiGroup.MapGet(pattern: "retrieve", async (Guid requestId, CancellationToken token,
            IGoogleAuthService authService) =>
        {
            var result = await authService.RetrieveTokensAsync(requestId, token);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NoContent();
        });
    }
}
