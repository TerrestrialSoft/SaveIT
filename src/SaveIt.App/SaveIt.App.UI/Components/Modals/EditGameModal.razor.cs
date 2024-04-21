using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models.Game;

namespace SaveIt.App.UI.Components.Modals;
public partial class EditGameModal
{
    public const string Title = "Edit Game";

    [Inject]
    private IGameRepository GameRepository { get; set; } = default!;

    [Inject]
    private IImageRepository ImageRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public Game Game { get; set; } = default!;

    [Parameter, EditorRequired]
    public required GameModel Model { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnSave { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnClose { get; set; }

    private Task CancelUpdateAsync() => OnClose.InvokeAsync();

    private async Task ValidSubmitAsync()
    {
        Game.Name = Model.Name;
        Game.Username = Model.Username;
        Game.GameExecutablePath = Model.GameExecutableFile?.FullPath;

        if (Model.Image?.ImageBase64 == Game.Image?.Content)
        {
            await GameRepository.UpdateGameAsync(Game);
            await OnSave.InvokeAsync();
            return;
        }

        if (Model.Image is null && Game.Image is not null)
        {
            await ImageRepository.DeleteImageAsync(Game.Image.Id);
            Game.Image = null;
            Game.ImageId = null;
        }
        else if (Model.Image is not null)
        {
            if(Game.Image is not null)
            {
                await ImageRepository.DeleteImageAsync(Game.Image.Id);
            }

            var image = new ImageEntity
            {
                Name = Model.Image.Name,
                Content = Model.Image.ImageBase64
            };
            await ImageRepository.CreateImageAsync(image);

            Game.Image = image;
            Game.ImageId = image.Id; 
        }

        await GameRepository.UpdateGameAsync(Game);
        await OnSave.InvokeAsync();
    }
}