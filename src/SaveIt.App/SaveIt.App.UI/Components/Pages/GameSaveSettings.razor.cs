using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Models;
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

    [Parameter]
    public string? GameSaveIdString { get; set; }

    private GameSave _gameSave = new();
    private GameSaveVersionsCountModel _model = new();
    private bool _updateInProgress = false;
    private bool _gameSaveExists = true;
    private Grid<FileItemModel> _grid = default!;
    private List<FileItemModel> files = default!;
    private FileItemModel _currentFile = default!;

    private Modal _localItemPickerModal = default!;
    private Modal _downloadGameSaveModal = default!;

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
            files = [];
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

        var save = await GameSaveRepository.GetGameSaveAsync(id);

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

    }

    private async Task ShowManualUploadModalAsync()
    {

    }
        
}