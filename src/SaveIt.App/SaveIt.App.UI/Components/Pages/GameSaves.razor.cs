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

    [Inject]
    public ToastService ToastMessageService { get; set; } = default!;

    private Grid<GameSaveViewModel> _grid = default!;
    private List<GameSaveViewModel> _gameSaves = default!;
    private NewGameSaveModel _createGameSave = new();
    private NewGameModel _createGame = new();
    private GameSave _editGameSave = new();

    private Modal _createGameModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _remoteItemPickerModal = default!;
    private Modal _authorizeStorageModal = default!;
    private Modal _createNewGameSaveModal = default!;
    private Modal _editGameSaveModal = default!;
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
        else if (_currentModal == _editGameSaveModal)
        {
            await ShowEditGameSaveModalAsync(_editGameSave);
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
                    ToastMessageService.Notify(new(ToastType.Success, "Game save created successfully."));
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
                    ToastMessageService.Notify(new(ToastType.Success, "New game created successfully."));
                })
            },
        };

        await _createGameModal.ShowAsync<CreateGameModal>(CreateGameModal.Title, parameters: parameters);
    }

    private async Task ShowEditGameSaveModalAsync(GameSave gameSave)
    {
        _currentModal = _editGameSaveModal;
        _editGameSave = gameSave;
        var model = _editGameSave.ToNewGameSaveModel();
        var parameters = new Dictionary<string, object>
        {
            { nameof(EditGameSaveModal.ModalCurrent), _editGameSaveModal },
            { nameof(EditGameSaveModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(EditGameSaveModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(EditGameSaveModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(EditGameSaveModal.GameSave), _editGameSave },
            { nameof(EditGameSaveModal.Model), model },
            { nameof(EditGameSaveModal.OnGameSaveUpdated),
                EventCallback.Factory.Create<GameSave>(this, async (gs) =>
                {
                    await _editGameSaveModal.HideAsync();
                    var index = _gameSaves.FindIndex(x => x.GameSave.Id == gs.Id);
                    _gameSaves[index] = gs.ToViewModel()!;
                    await _grid.RefreshDataAsync();
                    ToastMessageService.Notify(new(ToastType.Success, "Game save updated successfully."));
                })
            },
        };

        await _editGameSaveModal.ShowAsync<EditGameSaveModal>(EditGameSaveModal.Title, parameters: parameters);
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
        await _grid.RefreshDataAsync();
        ToastMessageService.Notify(new(ToastType.Success, "Game save deleted successfully."));
    }

    private Task ShowShareGameSaveModalAsync(GameSave gameSave)
    {
        throw new NotImplementedException();
    }

    private Task ShowAdvancedGameSaveSettingsModalAsync(GameSave gameSave)
    {
        throw new NotImplementedException();
    }
}