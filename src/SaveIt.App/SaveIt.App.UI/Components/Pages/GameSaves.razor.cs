using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals;

namespace SaveIt.App.UI.Components.Pages;
public partial class GameSaves
{
    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    private Grid<GameSave> _grid = default!;
    private List<GameSave> _gameSaves = default!;

    private Modal _createNewGameSaveModal = default!;
    private ConfirmDialog _confirmDialog = default!;

    private async Task<GridDataProviderResult<GameSave>> EmployeesDataProvider(GridDataProviderRequest<GameSave> request)
    {
        _gameSaves ??= (await GameSaveRepository.GetAllGameSavesWithChildrenAsync()).ToList();
        return await Task.FromResult(request.ApplyTo(_gameSaves));
    }

    private async Task ShowCreateNewGameSaveModal()
    {
        var parameters = new Dictionary<string, object>
        {
           
        };

        await _createNewGameSaveModal.ShowAsync<CreateGameSaveModal>(CreateGameSaveModal.Title, parameters: parameters);
    }

    private async Task ShowEditGameSaveModalAsync(GameSave gameSave)
    {
    }

    private async Task DeleteGameSaveAsync(GameSave gameSave)
    {
        var result = await _confirmDialog.ShowDeleteDialogAsync("Delete Game Save",
            "Are you sure you want to delete this game save?",
            "This action cannot be undone.");

        if (!result)
        {
            return;
        }

        await GameSaveRepository.DeleteGameSaveAsync(gameSave);
    }

    private async Task ShowShareGameSaveModalAsync(GameSave gameSave)
    {
    }

    private async Task ShowAdvancedGameSaveSettingsModalAsync(GameSave gameSave)
    {
    }
}