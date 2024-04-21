using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Modals;
public partial class EditGameSaveModal
{
    public const string Title = "Edit Game Save";

    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalRemoteItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalAuthorizeStorage { get; set; }

    [Parameter, EditorRequired]
    public required GameSave GameSave { get; set; }

    [Parameter, EditorRequired]
    public required NewGameSaveModel Model { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<GameSave> OnGameSaveUpdated { get; set; }

    private void SelectedGameChanged(Guid? id)
    {
        Model.GameId = id;
    }

    private async Task ValidSubmitAsync()
    {
        GameSave.GameId = Model.GameId!.Value;
        GameSave.Name = Model.GameSave.Name;
        GameSave.LocalGameSavePath = Model.GameSave.LocalGameSaveFile!.FullPath;
        GameSave.RemoteLocationId = Model.GameSave.RemoteGameSaveFile!.FullPath;
        GameSave.StorageAccountId = Model.GameSave.StorageAccountId!.Value;

        await GameSaveRepository.UpdateGameSaveAsync(GameSave);

        var gs = await GameSaveRepository.GetGameSaveWithChildrenAsync(GameSave.Id);

        if (gs is null)
        {
            return;
        }

        GameSave = gs;
        await OnGameSaveUpdated.InvokeAsync(GameSave);
    }

    private Task CancelUpdateAsync()
        => ModalCurrent.HideAsync();
}