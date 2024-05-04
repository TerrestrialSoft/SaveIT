using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Components.Modals.Utility;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Games;

namespace SaveIt.App.UI.Components.Modals.GameSaves;
public partial class DownloadGameSaveModal
{
    public const string Title = "Download Game Save";

    [Inject]
    private IGameService GameService { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Guid StorageAccountId { get; set; }

    [Parameter, EditorRequired]
    public required Guid GameSaveId { get; set; }

    [Parameter, EditorRequired]
    public required FileItemModel FileToDownload { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    private DownloadGameSaveModel _model = new()
    {
        SetAsActiveGameSave = true
    };

    private bool _isDownloading = false;
    private bool _isRepositoryLocked = false;
    private bool _initializing = false;

    protected override async Task OnInitializedAsync()
    {
        _initializing = true;
        var isLockedResult = await GameService.IsRepositoryLockedAsync(GameSaveId, CancellationToken);
        _initializing = false;

        if (isLockedResult.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, isLockedResult.Errors[0].Message));
            return;
        }

        _model.SetAsActiveGameSave = false;
        _isRepositoryLocked = isLockedResult.Value;
    }

    private Task HideModalAsync()
        => ModalCurrent.HideAsync();

    private async Task DownloadSaveAsync()
    {
        _isDownloading = true;
        StateHasChanged();

        var result = _model.SetAsActiveGameSave
            ? await GameService.PrepareSpecificGameSaveAsync(GameSaveId, FileToDownload.Id!, CancellationToken)
            : await GameService.DownloadGameSaveToSpecificLocationAsync(GameSaveId,
                FileToDownload.Id!, _model.LocalGameSaveFile!.FullPath, CancellationToken);

        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
            _isDownloading = false;
            await ModalCurrent.HideAsync();
            return;
        }

        ToastService.Notify(new ToastMessage(ToastType.Success, "Game Save downloaded successfully."));
        _isDownloading = false;
        _model = new DownloadGameSaveModel()
        {
            SetAsActiveGameSave = true
        };
        await ModalCurrent.HideAsync();
    }

    private async Task ShowLocalFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.InitialSelectedFile), _model.LocalGameSaveFile! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Folders },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Folders},
            { nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    _model.LocalGameSaveFile = file;
                    await ModalLocalItemPicker.HideAsync();
                })
            },
        };

        await ModalLocalItemPicker.ShowAsync<LocalItemPickerModal>(
            "Select the folder where you want to download the game save", parameters: parameters);
    }

    private void ClearLocalGameSavePath()
    {
        _model.LocalGameSaveFile = null;
    }
}