using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Modals;
public partial class StorageAuthorizationModal
{
    [Parameter, EditorRequired]
    public required EventCallback OnClose { get; set; }

    private AuthorizationScreenState _authState = AuthorizationScreenState.SelectProvider;

    private Task SelectGoogleProvider()
        => AuthorizeProviderAsync();

    private void BackToProviderSelection()
    {
        _authState = AuthorizationScreenState.SelectProvider;
    }

    private async Task AuthorizeProviderAsync()
    {
        _authState = AuthorizationScreenState.AuthorizationInProgress;

        var requestId = Guid.NewGuid();
        var url = await _authService.GetAuthorizationUrlAsync(requestId, CancellationToken);

        if (url.IsFailed)
        {
            _authState = AuthorizationScreenState.AuthorizeProviderError;
            return;
        }

        _navigationManager.NavigateTo(url.Value.ToString());
        var authorizationResult = await _authService.WaitForAuthorizationAsync(requestId, CancellationToken);

        _authState = authorizationResult.IsFailed
            ? AuthorizationScreenState.AuthorizeProviderError
            : AuthorizationScreenState.AuthorizeProviderSuccess;
    }

    private Task RetryAuthorization()
        => AuthorizeProviderAsync();

    private Task FinishAuthorization()
        => OnClose.InvokeAsync();

    private enum AuthorizationScreenState
    {
        SelectProvider,
        AuthorizationInProgress,
        AuthorizeProviderError,
        AuthorizeProviderSuccess
    }
}
