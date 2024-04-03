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

    private GameSave? SelectedSave { get; set; }


    private async Task ToggleDetailShowing()
    {
        ShowDetail = !ShowDetail;
        await OnCardClicked.InvokeAsync(Game);
    }
}
