using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Application.Services;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Games;

namespace SaveIt.App.UI.Components.Modals.Games;
public partial class CreateGameModal
{
    public const string Title = "Create New Game";

    [Inject]
    private ToastService ToastService { get; set; } = default!;

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
    public required NewGameModel CreateNewGameModel { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<Game> OnGameCreated { get; set; }

    private bool _isSaving = false;

    private async Task CreateGameAsync()
    {
        _isSaving = true;

        var game = new Game()
        {
            Id = Guid.NewGuid(),
            Name = CreateNewGameModel.Game.Name,
            Username = CreateNewGameModel.Game.Username,
            GameExecutablePath = CreateNewGameModel.Game.GameExecutableFile?.FullPath,
        };

        if (CreateNewGameModel.Game.Image is ImageModel img)
        {
            var image = new ImageEntity()
            {
                Id = Guid.NewGuid(),
                Name = img.Name,
                Content = img.ImageBase64
            };

            await ImageRepository.CreateAsync(image);
            game.ImageId = image.Id;
            game.Image = image;
        }

        var gameSave = new GameSave()
        {
            Id = Guid.NewGuid(),
            Name = CreateNewGameModel.GameSave.Name,
            GameId = game.Id,
            StorageAccountId = CreateNewGameModel.GameSave.StorageAccountId!.Value,
            RemoteLocationId = CreateNewGameModel.GameSave.RemoteGameSaveFile!.Id,
            RemoteLocationName = CreateNewGameModel.GameSave.RemoteGameSaveFile!.Name,
            LocalGameSavePath = CreateNewGameModel.GameSave.LocalGameSaveFile!.FullPath,
        };

        await GameRepository.CreateAsync(game);
        await GameSaveRepository.CreateAsync(gameSave);

        game.GameSaves = [gameSave];

        _isSaving = false;

        ToastService.Notify(new(ToastType.Success, "Game created successfully"));

        CreateNewGameModel = new();
        await ModalCurrent.HideAsync();
        await OnGameCreated.InvokeAsync(game);
    }
}