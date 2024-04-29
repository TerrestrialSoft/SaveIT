using BlazorBootstrap;
using FluentResults;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Game;
using System.Reflection;

namespace SaveIt.App.UI.Components.Modals;
public partial class DownloadGameSaveModal
{
    public const string Title = "Download Game Save";

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

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

    private Task HideModalAsync()
        => ModalCurrent.HideAsync();

    private async Task DownloadSaveAsync()
    {
        _isDownloading = true;
        StateHasChanged();

        var result = _model.SetAsActiveGameSave
            ? await GameService.PrepareSpecificGameSaveAsync(GameSaveId, FileToDownload.Id!)
            : await GameService.DownloadGameSaveToSpecificLocationAsync(GameSaveId,
                FileToDownload.Id!, _model.LocalGameSaveFile!.FullPath);

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