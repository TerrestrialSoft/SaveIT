using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals.Games;
using SaveIt.App.UI.Components.Modals.GameSaves;
using SaveIt.App.UI.Components.Pages;
using SaveIt.App.UI.Extensions;
using SaveIt.App.UI.Models.Games;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameCard
{
    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter, EditorRequired]
    public EditGameModel Model { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Game Game { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<Game> OnCardClicked { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnCardMinimized { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<Game> OnCardUpdated { get; set; }

    [Parameter]
    public bool ShowDetail { get; set; } = false;

    [Parameter, EditorRequired]
    public required Modal ModalStartGame { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalEditGame { get; set; }

    private ConfirmDialog _confirmDialog = default!;
    private GameSave? selectedSave;
    private bool _isGameStarting = false;
    private bool _isGameDeleting = false;

    protected override void OnAfterRender(bool firstRender)
    {
        if (Game.GameSaves is not null && Game.GameSaves.Count > 0)
        {
            selectedSave = selectedSave is not null
                ? Game.GameSaves.Find(x => x.Id == selectedSave.Id)
                : Game.GameSaves[0];
        }
    }

    private bool IsGameHosting()
        => Game.GameSaves?.Exists(x => x.IsHosting) ?? false;

    private async Task StartDetailShowing()
    {
        if (ShowDetail)
        {
            return;
        }

        ShowDetail = true;
        await OnCardClicked.InvokeAsync(Game);
    }

    private async Task StartGameAsync()
    {
        if(selectedSave is null)
        {
            ToastService.Notify(new(ToastType.Danger, $"No save found for game {Game.Name}"));
            return;
        }
        _isGameStarting = true;

        var accountId = Game.GameSaves!.First(x => x.Id == selectedSave.Id).StorageAccountId;
        var account = await StorageAccountRepository.GetAsync(accountId);

        if(account is null)
        {
            ToastService.Notify(new(ToastType.Danger, $"Storage account not found"));
            _isGameStarting = false;
            return;
        }

        if (!account.IsAuthorized)
        {
            ToastService.Notify(new(ToastType.Danger, $"Storage account not authorized",
                "Navigate to Storage accounts page and authorize the account."));
            _isGameStarting = false;
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            { nameof(StartGameSaveModal.GameSaveId), selectedSave.Id},
            { nameof(StartGameSaveModal.DefaultGameName), Game.Name },
            { nameof(StartGameSaveModal.OnClose), EventCallback.Factory.Create(this, async () =>
                {
                    await ModalStartGame.HideAsync();
                    await OnCardUpdated.InvokeAsync(Game);
                })
            },
            { nameof(StartGameSaveModal.OnGameSaveUpdate), EventCallback.Factory.Create(this, async (GameSave gs) =>
                {
                    await OnCardUpdated.InvokeAsync(gs.Game);
                })
            }
        };

        _isGameStarting = false;
        await ModalStartGame.ShowAsync<StartGameSaveModal>(StartGameSaveModal.Title, parameters: parameters);
    }

    private async Task DeleteGameAsync()
    {
        var result = await _confirmDialog.ShowDeleteDialogAsync($"Delete game {Game.Name}",
                $"Are you sure you want to delete {Game.Name}?",
                "This action cannot be undone.");

        if (!result)
        {
            return;
        }
        
        _isGameDeleting = true;
        await GameRepository.DeleteAsync(Game.Id, true);
        _isGameDeleting = false;

        ToastService.Notify(new(ToastType.Success, $"Game deleted successfully."));
        await OnCardUpdated.InvokeAsync(Game);
    }

    private async Task ShowEditGameModalAsync()
    {
        Model.GameModel = Game.ToEditGameModel()!;
        var parameters = new Dictionary<string, object>
        {
            { nameof(EditGameModal.Model), Model!},
            { nameof(EditGameModal.Game), Game},
            { nameof(EditGameModal.ModalLocalItemPicker), ModalLocalItemPicker},
            { nameof(EditGameModal.ModalCurrent), ModalEditGame},
            { nameof(EditGameModal.OnSave), EventCallback.Factory.Create(this, async () =>
                {
                    await ModalEditGame.HideAsync();
                    StateHasChanged();
                })
            },
            { nameof(EditGameModal.OnClose), EventCallback.Factory.Create(this, ModalEditGame.HideAsync) }
        };

        await ModalEditGame.ShowAsync<EditGameModal>(EditGameModal.Title, parameters: parameters);
    }

    private async Task MinimizeCardAsync()
    {
        if (!ShowDetail)
        {
            return;
        }

        ShowDetail = false;
        await OnCardMinimized.InvokeAsync();
    }

    private void CreateGameSaveRedirect()
    {
        var url = string.Format(GameSaves.CreateOperationTemplate, Game.Id);
        NavigationManager.NavigateTo(url);
    }

    private void EditGameSaveRedirect()
    {
        if (selectedSave is null)
        {
            return;
        }

        var url = string.Format(GameSaves.EditOperationTemplate, selectedSave.Id);
        NavigationManager.NavigateTo(url);
    }
}
