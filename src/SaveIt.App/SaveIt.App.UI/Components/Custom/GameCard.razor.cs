using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameCard
{
    [Parameter]
    public required Game Game { get; set; }

    [Parameter]
    public EventCallback<Game> OnCardClicked { get; set; }

    [Parameter]
    public bool ShowDetail { get; set; } = false;

    private Guid? SelectedSaveId { get; set; }

    protected override void OnInitialized()
    {
        if (Game.GameSaves is [] && Game.GameSaves.Count != 0)
            SelectedSaveId = Game.GameSaves[0].Id;
    }

    private async Task ToggleDetailShowing()
    {
        if(!ShowDetail)
        {
            ShowDetail = true;
            await OnCardClicked.InvokeAsync(Game);
        }
    }

    private void PlayGame()
    {

    }
}
