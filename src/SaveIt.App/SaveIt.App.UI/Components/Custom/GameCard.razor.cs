using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals;

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

    private ConfirmDialog confirmDialog = default!;

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

    private async Task StartGame()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(StartGameModal.SaveId), SelectedSaveId!.Value },
            { nameof(StartGameModal.DefaultGameName), Game.Name },
            { nameof(StartGameModal.OnClose), EventCallback.Factory.Create(this, ModalStartGame.HideAsync) }
        };

        await ModalStartGame.ShowAsync<StartGameModal>(StartGameModal.Title, parameters: parameters);
    }

    private async Task DeleteGame()
    {
        var options = new ConfirmDialogOptions
        {
            YesButtonText = "Delete",
            YesButtonColor = ButtonColor.Primary,
            NoButtonText = "Cancel",
            NoButtonColor = ButtonColor.Light,
            Size = DialogSize.Large,
            IsVerticallyCentered = true,
            DialogCssClass = "fs-5"
        };

        var confirmation = await confirmDialog.ShowAsync(
            title: $"Delete game {Game.Name}",
            message1: $"Are you sure you want to delete {Game.Name}?",
            message2: "This action cannot be undone.",
            options);

        if (!confirmation)
        {
            return;
        }

        await GameRepository.DeleteGameAsync(Game.Id);
        ToastService.Notify(new(ToastType.Success, $"Game deleted successfully."));
        await OnCardUpdated.InvokeAsync(Game);
    }
}
