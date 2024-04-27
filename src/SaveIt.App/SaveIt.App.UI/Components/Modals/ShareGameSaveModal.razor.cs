using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.UI.Models.Users;

namespace SaveIt.App.UI.Components.Modals;
public partial class ShareGameSaveModal
{
    public const string Title = "Share Game Save";

    [Inject]
    public ToastService ToastService { get; set; } = default!;
    
    [Inject]
    public IExternalStorageService ExternalStorageService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required GameSave GameSave { get; set; }

    [Parameter, EditorRequired]
    public required Guid StorageAccountId { get; set; }

    [Parameter, EditorRequired]
    public required string RemoteFileId { get; set; }

    private ShareWithCreateModel _model = new();
    private string? _error = null;
    Grid<ShareWithModel> grid = default!;
    private List<ShareWithModel> users = default!;
    private bool _shareInProgress = false;
    private bool _unshareInProgress = false;

    private ConfirmDialog confirmDialog = default!;

    private async Task<GridDataProviderResult<ShareWithModel>> ShareWithUsersDataProvider(
        GridDataProviderRequest<ShareWithModel> request)
    {
        if (users is null || users.Count == 0)
        {
            var result = await ExternalStorageService.GetSharedWithUsersForFile(StorageAccountId, RemoteFileId);

            if (result.IsFailed)
            {
                ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
                users = [];
                return await Task.FromResult(request.ApplyTo(users));
            }

            users = result.Value.ToList();
        }

        return await Task.FromResult(request.ApplyTo(users));
    }

    private async Task OnValidSubmitAsync()
    {
        _shareInProgress = true;
        _error = null;

        var result = await ExternalStorageService.ShareFileWithUserAsync(StorageAccountId, RemoteFileId, _model.Email);


        if (result.IsFailed)
        {
            _error = result.Errors[0].Message;
            _shareInProgress = false;
            _model.Email = string.Empty;

            return;
        }

        _model = new ShareWithCreateModel();

        users = [];
        await grid.RefreshDataAsync();
        _shareInProgress = false;
    }

    private async Task StopSharingAsync(ShareWithModel user)
    {
        if(_unshareInProgress)
        {
            return;
        }
        
        if (user.IsOwner)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, "You cannot remove the owner of the file."));
            return;
        }

        var dialogResult = await confirmDialog.ShowDeleteDialogAsync($"Stop sharing with {user.Username} ({user.Email})",
            "Are you sure you want to stop sharing this file with this user?",
            "This operation cannot be undone.");

        if (!dialogResult)
        {
            return;
        }

        _unshareInProgress = true;
        StateHasChanged();

        var result = await ExternalStorageService.StopSharingFileWithUserAsync(StorageAccountId, RemoteFileId, user.PermissionId);

        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
            _unshareInProgress = false;
            return;
        }

        ToastService.Notify(new ToastMessage(ToastType.Success, "User removed from sharing list."));
        _unshareInProgress = false;
        users = [];
        await grid.RefreshDataAsync();
    }
}