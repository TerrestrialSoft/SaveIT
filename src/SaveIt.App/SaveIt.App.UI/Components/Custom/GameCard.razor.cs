using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals;
using SaveIt.App.UI.Extensions;
using SaveIt.App.UI.Models.Game;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameCard
{
    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

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

    private readonly ConfirmDialog confirmDialog = default!;

    private Guid? SelectedSaveId { get; set; }

    protected override void OnInitialized()
    {
        if (Game.GameSaves is not null && Game.GameSaves.Count > 0)
            SelectedSaveId = Game.GameSaves[0].Id;
    }

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
        var parameters = new Dictionary<string, object>
        {
            { nameof(StartGameModal.SaveId), SelectedSaveId!.Value },
            { nameof(StartGameModal.DefaultGameName), Game.Name },
            { nameof(StartGameModal.OnClose), EventCallback.Factory.Create(this, ModalStartGame.HideAsync) }
        };

        await ModalStartGame.ShowAsync<StartGameModal>(StartGameModal.Title, parameters: parameters);
    }

    private async Task DeleteGameAsync()
    {
        var result = await confirmDialog.ShowDeleteDialogAsync($"Delete game {Game.Name}",
                $"Are you sure you want to delete {Game.Name}?",
                "This action cannot be undone.");

        if (!result)
        {
            return;
        }

        await GameRepository.DeleteGameAsync(Game.Id);
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
