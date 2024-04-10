using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Pages;
public partial class Games
{
    private Modal _createNewGameModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _remoteItemPickerModal = default!;
    private Modal _authorizeStorageModal = default!;

    private List<Game> _allGames = [];
    private List<Game> _filteredGames = [];
    private string _searchText = "";
    private Game? _selectedGame = new();
    private NewGameModel _createGame = new();

    protected override async Task OnInitializedAsync()
    {
        await RefreshGames();
    }

    private async Task RefreshGames()
    {
        _allGames = (await gameRepository.GetAllGamesAsync()).ToList();
        UpdateGames(_searchText);
    }

    private void UpdateGames(string searchText)
        => _filteredGames = _allGames.Where(g => g.Name.Contains(searchText)).ToList();

    private void GameCardClicked(Game g)
        => _selectedGame = g;

    private Task ShowNewCreateNewGameModal()
    {
        _createGame = new();
        return ShowCreateNewGameModal();
    }

    private async Task ShowCreateNewGameModal()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameModal.ModalCurrent), _createNewGameModal },
            { nameof(CreateGameModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(CreateGameModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(CreateGameModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(CreateGameModal.EditGame), _createGame },
            { nameof(CreateGameModal.OnGameCreated),
                EventCallback.Factory.Create(this, async () =>
                {
                    await RefreshGames();
                })
            },
        };

        await _createNewGameModal.ShowAsync<CreateGameModal>(CreateGameModal.Title, parameters: parameters);
    }
}
