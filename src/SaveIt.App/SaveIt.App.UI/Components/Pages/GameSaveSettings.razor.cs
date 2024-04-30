using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Pages;
public partial class GameSaveSettings
{
    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    public PreloadService PreloadService { get; set; } = default!;

    [Parameter]
    public string? GameSaveIdString { get; set; }

    private GameSave _gameSave = new();
    private UploadGameSaveModel _uploadModel = new();
    private GameSaveVersionsCountModel _versionsModel = new();
    private bool _updateInProgress = false;
    private bool _gameSaveExists = true;
    private Grid<FileItemModel> _grid = default!;
    private List<FileItemModel> files = default!;
    private FileItemModel _currentFile = default!;

    private Modal _uploadFolderPickerModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _downloadGameSaveModal = default!;
    private Modal _uploadFolderAsGameSaveModal = default!;
    private ConfirmDialog _confirmDialog = default!;

    private async Task<GridDataProviderResult<FileItemModel>> GameSaveVersionsProvider(
        GridDataProviderRequest<FileItemModel> request)
    {
        if (files is not null && files.Count != 0)
        {
            return await Task.FromResult(request.ApplyTo(files));
        }

        var result = await GameService.GetGameSaveVersionsAsync(_gameSave.StorageAccountId, _gameSave.RemoteLocationId);

        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
            StateHasChanged();
            files.Clear();
            return await Task.FromResult(request.ApplyTo(files));
        }

        files = result.Value.ToList();
        StateHasChanged();

        return await Task.FromResult(request.ApplyTo(files));
    }

    protected override async Task OnInitializedAsync()
    {
        if(!Guid.TryParse(GameSaveIdString, out var id))
        {
            _gameSaveExists = false;
            return;
        }

        var save = await GameSaveRepository.GetAsync(id);

        if (save is null)
        {
            _gameSaveExists = false;
            return;
        }

        _gameSave = save;
    }

    private async Task ShowDownloadSaveModalAsync(FileItemModel file)
    {
        _currentFile = file;
        var parameters = new Dictionary<string, object>
        {
            { nameof(DownloadGameSaveModal.ModalCurrent), _downloadGameSaveModal },
            { nameof(DownloadGameSaveModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(DownloadGameSaveModal.StorageAccountId), _gameSave.StorageAccountId },
            { nameof(DownloadGameSaveModal.GameSaveId), _gameSave.Id },
            { nameof(DownloadGameSaveModal.FileToDownload), _currentFile },
        };

        await _downloadGameSaveModal.ShowAsync<DownloadGameSaveModal>($"{DownloadGameSaveModal.Title} {file.Name}",
            parameters: parameters);
    }

    private Task ShowDownloadGameSaveModalAsync()
        => ShowDownloadSaveModalAsync(_currentFile);

    private async Task ChangeVersionsCount()
    {
    }

    private async Task UnlockRepositoryAsync()
    {
        var dialogResult = await _confirmDialog.ShowDialogAsync("Unlock Repository",
                       "Are you sure you want to unlock the repository? You risk current host progress.",
                       "This action cannot be undone.",
                       "Unlock",
                       "No");

        if (!dialogResult)
        {
            return;
        }

        PreloadService.Show();
        var result = await GameService.UnlockRepositoryAsync(_gameSave.Id);
        PreloadService.Hide();
        
        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
            return;
        }

        ToastService.Notify(new ToastMessage(ToastType.Success, "Repository unlocked successfully."));
    }

    private async Task ShowUploadGameSaveModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(UploadGameSaveModal.ModalCurrent), _uploadFolderAsGameSaveModal },
            { nameof(UploadGameSaveModal.ModalLocalItemPicker), _uploadFolderPickerModal },
            { nameof(UploadGameSaveModal.GameSaveId), _gameSave.Id },
            { nameof(UploadGameSaveModal.Model), _uploadModel }
        };

        await _uploadFolderAsGameSaveModal.ShowAsync<UploadGameSaveModal>(UploadGameSaveModal.Title, parameters: parameters);
    }
}