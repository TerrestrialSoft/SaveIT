using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Modals;
public partial class GameSaveVersionsModal
{
    public const string Title = "Game Save Versions";

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required GameSave GameSave { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalDownloadGameSave { get; set; }

    private GameSaveVersionsCountModel _model = new();
    private Grid<FileItemModel> _grid = default!;
    private List<FileItemModel> files = default!;
    private bool _updateInProgress = false;
    private string? _error = null;


    private async Task<GridDataProviderResult<FileItemModel>> GameSaveVersionsProvider(
        GridDataProviderRequest<FileItemModel> request)
    {
        if (files is not null && files.Count != 0)
        {
            return await Task.FromResult(request.ApplyTo(files));
        }

        var result = await GameService.GetGameSaveVersionsAsync(GameSave.StorageAccountId, GameSave.RemoteLocationId);

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

    private async Task OnValidSubmitAsync()
    {
        throw new NotImplementedException();
    }

    private async Task ShowDownloadSaveAsync(FileItemModel file)
    {

        //await ModalCurrent.HideAsync();
        //var parameters = new Dictionary<string, object>
        //{
        //    { nameof(DownloadGameSaveModal.StorageAccountId), GameSave.StorageAccountId },
        //    { nameof(DownloadGameSaveModal.FileItem), file },
        //    {nameof }
        //};

        //await ModalDownloadGameSave.ShowAsync<DownloadGameSaveModal>(DownloadGameSaveModal.Title, parameters: parameters);
    }
}