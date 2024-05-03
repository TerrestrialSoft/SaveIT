using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Modals.GameSaves;
public partial class EditGameSaveModal
{
    public const string Title = "Edit Game Save";

    [Inject]
    private IGameSaveRepository GameSaveRepository { get; set; } = default!;

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

    private bool _isSaving = false;

    private void SelectedGameChanged(Guid? id)
    {
        Model.GameId = id;
    }

    private async Task UpdateGameSaveAsync()
    {
        _isSaving = true;
        GameSave.GameId = Model.GameId!.Value;
        GameSave.Name = Model.GameSave.Name;
        GameSave.LocalGameSavePath = Model.GameSave.LocalGameSaveFile!.FullPath;
        GameSave.RemoteLocationId = Model.GameSave.RemoteGameSaveFile!.Id;
        GameSave.RemoteLocationName = Model.GameSave.RemoteGameSaveFile!.Name;
        GameSave.StorageAccountId = Model.GameSave.StorageAccountId!.Value;

        await GameSaveRepository.UpdateAsync(GameSave);

        var gs = await GameSaveRepository.GetWithChildrenAsync(GameSave.Id);

        _isSaving = false;

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