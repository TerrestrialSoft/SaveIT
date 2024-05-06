using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals.Games;
using SaveIt.App.UI.Components.Modals.GameSaves;
using SaveIt.App.UI.Extensions;
using SaveIt.App.UI.Models.Games;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Pages;
public partial class GameSaves
{
    public const string CreateOperationTemplate = "/game-saves?operation=create&id={0}";
    public const string EditOperationTemplate = "/game-saves?operation=edit&id={0}";
    private const string GameSaveSettingsUrl = "/game-saves/{0}";

    [Inject]
    private IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Inject]
    private ToastService ToastMessageService { get; set; } = default!;

    [Inject]
    private NavigationManager NavManager { get; set; } = default!;

    [Inject]
    private PreloadService PreloadService { get; set; } = default!;

    private Grid<GameSaveViewModel> _grid = default!;
    private List<GameSaveViewModel> _gameSaves = default!;
    private readonly NewGameSaveModel _createGameSave = new();
    private readonly NewGameModel _createGame = new();
    private GameSave _currentGameSave = new();
    private NewGameSaveModel _currentGameSaveModel = new();
    private Modal _createGameModal = default!;
    private Modal _localItemPickerModal = default!;
    private Modal _remoteItemPickerModal = default!;
    private Modal _authorizeStorageModal = default!;
    private Modal _createNewGameSaveModal = default!;
    private Modal _editGameSaveModal = default!;
    private Modal _currentModal = default!;
    private Modal _shareGameSaveModal = default!;

    private ConfirmDialog _confirmDialog = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!NavManager.TryGetQueryParameter<string>("operation", out var operation) || operation is null)
        {
            return;
        }

        if (!NavManager.TryGetQueryParameter<Guid>("id", out var id))
        {
            return;
        }

        switch (operation.ToLower())
        {
            case "create":
                await ShowCreateGameSaveModalAsync(id);
                break;
            case "edit":
                var gameSave = await GameSaveRepository.GetWithChildrenAsync(id);
                if (gameSave is null)
                {
                    return;
                }
                await ShowEditGameSaveModalAsync(gameSave);
                break;
        }
    }

    private async Task<GridDataProviderResult<GameSaveViewModel>> GameSavesDataProvider(
        GridDataProviderRequest<GameSaveViewModel> request)
    {
        if (_gameSaves is not null && _gameSaves.Count > 0)
        {
            return await Task.FromResult(request.ApplyTo(_gameSaves));
        }

        var gs = await GameSaveRepository.GetAllWithChildrenAsync();
        _gameSaves = gs.Select(x => x.ToViewModel()!).ToList();

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
            await ShowEditGameSaveModalAsync(_currentGameSave);
        } 
    }

    private async Task ShowCreateGameSaveModalAsync(Guid? gameId = null)
    {
        _currentModal = _createNewGameSaveModal;
        _createGameSave.GameId ??= gameId;

        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameSaveModal.ModalCurrent), _currentModal },
            { nameof(CreateGameSaveModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(CreateGameSaveModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(CreateGameSaveModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(CreateGameSaveModal.InitialGameId), _createGameSave.GameId! },
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

        await _currentModal.ShowAsync<CreateGameSaveModal>(CreateGameSaveModal.Title, parameters: parameters);
    }

    private async Task ShowCreateGameModalAsync()
    {
        _currentModal = _createGameModal;
        var parameters = new Dictionary<string, object>
        {
            { nameof(CreateGameModal.ModalCurrent), _currentModal },
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

        await _currentModal.ShowAsync<CreateGameModal>(CreateGameModal.Title, parameters: parameters);
    }

    private async Task ShowEditGameSaveModalAsync(GameSave gameSave)
    {
        _currentModal = _editGameSaveModal;
        if(_currentGameSave.Id == Guid.Empty)
        {
            _currentGameSave = gameSave;
            _currentGameSaveModel = gameSave.ToNewGameSaveModel();
        }
        var parameters = new Dictionary<string, object>
        {
            { nameof(EditGameSaveModal.ModalCurrent), _currentModal },
            { nameof(EditGameSaveModal.ModalLocalItemPicker), _localItemPickerModal },
            { nameof(EditGameSaveModal.ModalRemoteItemPicker), _remoteItemPickerModal },
            { nameof(EditGameSaveModal.ModalAuthorizeStorage), _authorizeStorageModal },
            { nameof(EditGameSaveModal.GameSave), _currentGameSave },
            { nameof(EditGameSaveModal.Model), _currentGameSaveModel },
            { nameof(EditGameSaveModal.OnGameSaveUpdated),
                EventCallback.Factory.Create<GameSave>(this, async (gs) =>
                {
                    await _editGameSaveModal.HideAsync();
                    var index = _gameSaves.FindIndex(x => x.GameSave.Id == gs.Id);
                    _gameSaves[index] = gs.ToViewModel()!;
                    await _grid.RefreshDataAsync();
                    _currentGameSave = new();
                    _currentGameSaveModel = new();
                    ToastMessageService.Notify(new(ToastType.Success, "Game save updated successfully."));
                })
            },
        };

        await _currentModal.ShowAsync<EditGameSaveModal>(EditGameSaveModal.Title, parameters: parameters);
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

        PreloadService.Show();
        await GameSaveRepository.DeleteAsync(gameSave.Id);
        PreloadService.Hide();

        _gameSaves.Clear();
        await _grid.RefreshDataAsync();
        ToastMessageService.Notify(new(ToastType.Success, "Game save deleted successfully."));
    }

    private async Task ShowShareGameSaveModalAsync(GameSave gameSave)
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(ShareGameSaveModal.GameSave), gameSave },
            { nameof(ShareGameSaveModal.StorageAccountId), gameSave.StorageAccountId },
            { nameof(ShareGameSaveModal.RemoteFileId), gameSave.RemoteLocationId },
        };

        await _shareGameSaveModal.ShowAsync<ShareGameSaveModal>(ShareGameSaveModal.Title, parameters: parameters);
    }

    private void ShowAdvancedGameSaveSettings(GameSave gameSave)
    {
        var url = string.Format(GameSaveSettingsUrl, gameSave.Id);
        NavManager.NavigateTo(url);
    }

    private void ClearParameters()
    {
        if (NavManager.TryGetQueryParameter<string>("operation", out var _))
        {
            NavManager.NavigateTo("/game-saves");
        }
    }
}