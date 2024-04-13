using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameCard
{
    [Inject]
    private IProcessService ProcessService { get; set; } = default!;

    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IGameService GameService { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Parameter]
    public required Game Game { get; set; }

    [Parameter]
    public EventCallback<Game> OnCardClicked { get; set; }

    [Parameter]
    public EventCallback<Game> OnCardUpdated { get; set; }

    [Parameter]
    public bool ShowDetail { get; set; } = false;

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

    private void StartGame()
    {
        GameService.LockRepositoryAsync(SelectedSaveId!.Value);
        return;
        if (Game.GameExecutablePath is null)
        {
            return;
        }

        var result = ProcessService.StartProcess(Game.GameExecutablePath);
        
        var message = result.IsSuccess
            ? new ToastMessage(ToastType.Success, $"Game started successfully.")
            : new ToastMessage(ToastType.Danger, "Failed to start the game.");

        ToastService.Notify(message);
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
