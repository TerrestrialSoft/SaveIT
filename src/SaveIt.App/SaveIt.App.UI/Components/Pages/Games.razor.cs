using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Models.Games;

namespace SaveIt.App.UI.Components.Pages;
public partial class Games
{
    private Modal _createGameModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _remoteItemPickerModal = default!;
    private Modal _authorizeStorageModal = default!;
    private Modal _startGameModal = default!;
    private Modal _editGameModal = default!;

    private List<GameCardModel> _allGames = [];
    private List<GameCardModel> _filteredGames = [];
    private readonly string _searchText = "";
    private GameCardModel? _selectedGame;
    private NewGameModel _createGame = new();

    protected override async Task OnInitializedAsync()
        => await RefreshGamesAsync();

    private async Task RefreshGamesAsync()
    {
        _allGames = (await gameRepository.GetAllWithChildrenAsync()).Select(x => new GameCardModel(x)).ToList();
        UpdateGames(_searchText);
    }

    private void UpdateGames(string searchText)
        => _filteredGames = _allGames.Where(g => g.Game.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

    private void GameCardClicked(Game g)
    {
        GameCardUpdated(g);
    }

    private Task ShowNewCreateNewGameModal()
    {
        _createGame = new();
        return ShowCreateNewGameModal();
    }

    private async Task ShowCreateNewGameModal()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameModal.ModalCurrent), _createGameModal },
            { nameof(CreateGameModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(CreateGameModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(CreateGameModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(CreateGameModal.CreateNewGameModel), _createGame },
            { nameof(CreateGameModal.OnGameCreated),
                EventCallback.Factory.Create<Game>(this, async (game) => await RefreshGamesAsync())
            },
        };

        await _createGameModal.ShowAsync<CreateGameModal>(CreateGameModal.Title, parameters: parameters);
    }

    private void GameCardUpdated(Game game)
    {
        _selectedGame = new GameCardModel(game);
        _allGames[_allGames.FindIndex(g => g.Game.Id == game.Id)] = _selectedGame;
        _filteredGames[_filteredGames.FindIndex(g => g.Game.Id == game.Id)] = _selectedGame;
    }

    private async Task GameCardDataUpdatedAsync(Game game)
    {
        game = (await gameRepository.GetWithChildrenAsync(game.Id))!;
        GameCardUpdated(game);
    }

    private void GameCardMinimized()
    {
        _selectedGame = null;
    }
}
