using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Extensions;
using SaveIt.App.UI.Models.Game;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Pages;
public partial class GameSaves
{
    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    private Grid<GameSaveViewModel> _grid = default!;
    private List<GameSaveViewModel> _gameSaves = default!;
    private NewGameSaveModel _createGameSave = new();
    private NewGameModel _createGame = new();

    private Modal _createGameModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _remoteItemPickerModal = default!;
    private Modal _authorizeStorageModal = default!;
    private Modal _createNewGameSaveModal = default!;
    private ConfirmDialog _confirmDialog = default!;

    private Modal _currentModal = default!;

    private async Task<GridDataProviderResult<GameSaveViewModel>> EmployeesDataProvider(
        GridDataProviderRequest<GameSaveViewModel> request)
    {
        _gameSaves ??= (await GameSaveRepository.GetAllGameSavesWithChildrenAsync()).Select(x => x.ToViewModel()!).ToList();
        return await Task.FromResult(request.ApplyTo(_gameSaves));
    }

    private async Task ShowModalAsync()
    {
        if (_currentModal == _createNewGameSaveModal)
        {
            await ShowCreateGameSaveModalAsync();
        }
        else if (_currentModal == _createGameModal)
        {
            await ShowCreateGameModalAsync();
        }
    }

    private async Task ShowCreateGameSaveModalAsync()
    {
        _currentModal = _createNewGameSaveModal;
        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameSaveModal.ModalCurrent), _createNewGameSaveModal },
            { nameof(CreateGameSaveModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(CreateGameSaveModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(CreateGameSaveModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(CreateGameSaveModal.GameSaveModel), _createGameSave},
            { nameof(CreateGameSaveModal.OnCreateGameRequested), EventCallback.Factory.Create(this, async () =>
                {
                    await _createNewGameSaveModal.HideAsync();
                    await ShowCreateGameModalAsync();
                })
            },
            { nameof(CreateGameSaveModal.OnGameSaveCreated), EventCallback.Factory.Create<GameSave>(this, async (gs) =>
                {
                    await _createNewGameSaveModal.HideAsync();
                    _gameSaves.Add(gs.ToViewModel()!);
                    await _grid.RefreshDataAsync();
                })
            }
        };

        await _createNewGameSaveModal.ShowAsync<CreateGameSaveModal>(CreateGameSaveModal.Title, parameters: parameters);
    }

    private async Task ShowCreateGameModalAsync()
    {
        _currentModal = _createGameModal;
        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameModal.ModalCurrent), _createGameModal },
            { nameof(CreateGameModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(CreateGameModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(CreateGameModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(CreateGameModal.CreateNewGameModel), _createGame },
            { nameof(CreateGameModal.OnGameCreated),
                EventCallback.Factory.Create<Game>(this, async (game) =>
                {
                    await _createGameModal.HideAsync();
                    _gameSaves.Add(game.GameSaves![0].ToViewModel()!);
                    await _grid.RefreshDataAsync();
                })
            },
        };

        await _createGameModal.ShowAsync<CreateGameModal>(CreateGameModal.Title, parameters: parameters);
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