using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.UI.Components.Custom;
public partial class GameSelect
{
    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public required EventCallback OnCreateGameRequested { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<Guid?> OnSelectedGameChanged { get; set; }

    private List<Game> _games = [];
    private Guid? _selectedGameId;

    protected override async Task OnInitializedAsync()
    {
        _games = (await GameRepository.GetAllGamesAsync()).ToList();
        _selectedGameId = _games.FirstOrDefault()?.Id;
        await OnSelectedGameChanged.InvokeAsync(_selectedGameId);
    }

    private Task GameChangedAsync(Guid? id)
    {
        _selectedGameId = id;
        return OnSelectedGameChanged.InvokeAsync(_selectedGameId);
    }

    private Task ShowCreateGameSaveModal() => OnCreateGameRequested.InvokeAsync();
}