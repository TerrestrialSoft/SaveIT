using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Game;

namespace SaveIt.App.UI.Components.Modals;
public partial class CreateGameModal
{
    public const string Title = "Create New Game";

    [Inject]
    protected ToastService ToastService { get; set; } = default!;

    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IImageRepository ImageRepository { get; set; } = default!;

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
    public required NewGameModel CreateNewCompleteGame { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnGameCreated { get; set; }

    private async Task Submit()
    {
        var game = new Game()
        {
            Id = Guid.NewGuid(),
            Name = CreateNewCompleteGame.Game.Name,
            Username = CreateNewCompleteGame.Game.Username,
            GameExecutablePath = CreateNewCompleteGame.Game.GameExecutableFile?.FullPath,
        };

        if(CreateNewCompleteGame.Game.Image is ImageModel img)
        {
            var image = new ImageEntity()
            {
                Id = Guid.NewGuid(),
                Name = img.Name,
                Content = img.ImageBase64
            };

            game.ImageId = image.Id;
            await ImageRepository.CreateImageAsync(image);
        }

        var gameSave = new GameSave()
        {
            Id = Guid.NewGuid(),
            Name = CreateNewCompleteGame.GameSave.Name,
            GameId = game.Id,
            StorageAccountId = CreateNewCompleteGame.GameSave.StorageAccountId!.Value,
            RemoteLocationId = CreateNewCompleteGame.GameSave.RemoteGameSaveFile!.Id,
            LocalGameSavePath = CreateNewCompleteGame.GameSave.LocalGameSaveFile!.FullPath,
        };
        
        await GameRepository.CreateGameAsync(game);
        await GameSaveRepository.CreateGameSaveAsync(gameSave);
        
        ToastService.Notify(new(ToastType.Success, "Game created successfully"));

        CreateNewCompleteGame = new();
        await ModalCurrent.HideAsync();
        await OnGameCreated.InvokeAsync();
    }
}