using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Modals;
public partial class CreateGameSaveModal
{
    public const string Title = "Create Game Save";
    [Inject]
    protected ToastService ToastService { get; set; } = default!;

    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalRemoteItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalAuthorizeStorage { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required NewGameSaveModel GameSaveModel { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GameSave> OnGameSaveCreated { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnCreateGameRequested { get; set; }

    private List<Game> _games = [];

    protected override async Task OnInitializedAsync()
    {
        _games = (await GameRepository.GetAllGamesAsync()).ToList();
        GameSaveModel.GameId= _games.FirstOrDefault()?.Id;
    }

    private Task ShowCreateGameSaveModal() => OnCreateGameRequested.InvokeAsync();

    private async Task ValidSubmit()
    {
        var gameSave = new GameSave()
        {
            Id = Guid.NewGuid(),
            Name = GameSaveModel.GameSave.Name,
            LocalGameSavePath = GameSaveModel.GameSave.LocalGameSaveFile!.FullPath,
            RemoteLocationId = GameSaveModel.GameSave.RemoteGameSaveFile!.FullPath,
            GameId = GameSaveModel.GameId!.Value,
            StorageAccountId = GameSaveModel.GameSave.StorageAccountId!.Value
        };

        await GameSaveRepository.CreateGameSaveAsync(gameSave);

        gameSave = await GameSaveRepository.GetGameSaveWithChildrenAsync(gameSave.Id);
        await OnGameSaveCreated.InvokeAsync(gameSave);
    }
}