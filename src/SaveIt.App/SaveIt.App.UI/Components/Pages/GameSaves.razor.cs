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

    private List<GameSave> _gameSaves = [];
    private List<GameSave> _filteredGameSaves = [];
    private Modal _createNewGameSaveModal = default!;

    protected override async Task OnInitializedAsync()
    {
        _gameSaves = (await GameSaveRepository.GetAllGameSaveAsync()).ToList();
        _filteredGameSaves = _gameSaves;
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
        _filteredGameSaves = _gameSaves.Where(g => g.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
    }
}