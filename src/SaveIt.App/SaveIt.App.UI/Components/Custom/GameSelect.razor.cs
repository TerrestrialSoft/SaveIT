using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameSelect
{
    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Parameter]
    public EventCallback OnCreateGameRequested { get; set; }

    [Parameter]
    public Guid? SelectedGameId { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<Guid?> OnSelectedGameChanged { get; set; }

    private List<Game> _games = [];
    private Guid? _selectedGameId;
    private bool IsOnCreateGameRequestSet => OnCreateGameRequested.HasDelegate;

    protected override async Task OnInitializedAsync()
    {
        _games = (await GameRepository.GetAllAsync()).ToList();

        if (SelectedGameId is not null)
        {
            _selectedGameId = SelectedGameId;
        }
        else
        {
            _selectedGameId = _games.FirstOrDefault()?.Id;
            await OnSelectedGameChanged.InvokeAsync(_selectedGameId);
        }
    }

    private Task GameChangedAsync(Guid? id)
    {
        _selectedGameId = id;
        return OnSelectedGameChanged.InvokeAsync(_selectedGameId);
    }

    private async Task TriggerOnCreateGameRequestedAsync()
    {
        if(IsOnCreateGameRequestSet)
        {
            return;
        }

        await OnCreateGameRequested.InvokeAsync();
    }
}