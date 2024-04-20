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

    private Modal _createNewGameSaveModal = default!;
    private Grid<GameSave> grid = default!;
    private List<GameSave> gameSaves = default!;

    private async Task<GridDataProviderResult<GameSave>> EmployeesDataProvider(GridDataProviderRequest<GameSave> request)
    {
        gameSaves ??= (await GameSaveRepository.GetAllGameSavesWithChildrenAsync()).ToList();
        return await Task.FromResult(request.ApplyTo(gameSaves));
    }

    private async Task ShowCreateNewGameSaveModal()
    {
        var parameters = new Dictionary<string, object>
        {
           
        };

        await _createNewGameSaveModal.ShowAsync<CreateGameSaveModal>(CreateGameSaveModal.Title, parameters: parameters);
    }

    private void UpdateGameSaves(string filter)
    {
    }
}