using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Services;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Modals;
public partial class UploadGameSaveModal
{
    public const string Title = "Upload Folder as Game Save";

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Parameter, EditorRequired]
    public Modal ModalCurrent { get; set; } = default!;

    [Parameter, EditorRequired]
    public Modal ModalLocalItemPicker { get; set; } = default!;

    [Parameter, EditorRequired]
    public Guid GameSaveId { get; set; }

    [Parameter, EditorRequired]
    public UploadGameSaveModel Model { get; set; } = default!;

    private bool _isUploading = false;

    private async Task UploadGameSaveAsync()
    {
        _isUploading = true;
        StateHasChanged();

        var result = await GameService.UploadFolderAsGameSaveAsync(GameSaveId, Model.File!.FullPath);
        
        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, result.Errors[0].Message));
            Model = new UploadGameSaveModel();
            _isUploading = false;
            return;
        }

        ToastService.Notify(new ToastMessage(ToastType.Success, "Folder uploaded successfully"));
        Model = new UploadGameSaveModel();
        _isUploading = false;
        await ModalCurrent.HideAsync();
    }

    private async Task ShowLocalFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.InitialSelectedFile), Model.File! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Folders },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Folders},
            { nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    Model!.File = file;
                    await ModalLocalItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalItemPicker.ShowAsync<LocalItemPickerModal>("Select Game Save Folder", parameters: parameters);
    }

    private Task HideModalAsync()
        => ModalCurrent.HideAsync();

    private void ClearLocalGameSavePath()
    {
        Model.File = null;
    }

}