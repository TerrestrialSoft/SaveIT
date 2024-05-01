using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Extensions;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameCard
{
    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Game Game { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<Game> OnCardClicked { get; set; }

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
    private string _startGameButtonText = "";

    protected override void OnAfterRender(bool firstRender)
    {
        if (Game.GameSaves is not null && Game.GameSaves.Count > 0)
        {
            selectedSave = selectedSave is not null
                ? Game.GameSaves.Find(x => x.Id == selectedSave.Id)
                : Game.GameSaves[0];
        }
    }

    private bool IsGameHosting() => Game.GameSaves?.Exists(x => x.IsHosting) ?? false;

    private async Task ToggleDetailShowing()
    {
        if (!ShowDetail)
        {
            ShowDetail = true;
            await OnCardClicked.InvokeAsync(Game);
        }
    }

    private async Task StartGameAsync()
    {
        if(selectedSave is null)
        {
            ToastService.Notify(new(ToastType.Danger, $"No save found for game {Game.Name}"));
            return;
        }

        var accountId = Game.GameSaves!.First(x => x.Id == selectedSave.Id).StorageAccountId;
        var account = await StorageAccountRepository.GetAsync(accountId);

        if(account is null)
        {
            ToastService.Notify(new(ToastType.Danger, $"Storage account not found"));
            return;
        }

        if (!account.IsAuthorized)
        {
            ToastService.Notify(new(ToastType.Danger, $"Storage account not authorized",
                "Navigate to Storage accounts page and authorize the account."));
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            { nameof(StartGameModal.SaveId), selectedSave.Id},
            { nameof(StartGameModal.DefaultGameName), Game.Name },
            { nameof(StartGameModal.OnClose), EventCallback.Factory.Create(this, async () =>
                {
                    await ModalStartGame.HideAsync();
                    await OnCardUpdated.InvokeAsync(Game);
                })
            },
            { nameof(StartGameModal.OnGameSaveUpdate), EventCallback.Factory.Create(this, async (GameSave gs) =>
                {
                    await OnCardUpdated.InvokeAsync(gs.Game);
                })
            }
        };

        await ModalStartGame.ShowAsync<StartGameModal>(StartGameModal.Title, parameters: parameters);
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

        await GameRepository.DeleteAsync(Game.Id, true);
        ToastService.Notify(new(ToastType.Success, $"Game deleted successfully."));
        await OnCardUpdated.InvokeAsync(Game);
    }

    private async Task ShowEditGameModalAsync()
    {
        var gameModel = Game.ToEditGameModel();
        var parameters = new Dictionary<string, object>
        {
            { nameof(EditGameModal.Model), gameModel!},
            { nameof(EditGameModal.Game), Game},
            { nameof(EditGameModal.ModalLocalItemPicker), ModalLocalItemPicker},
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
}
