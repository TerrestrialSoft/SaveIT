using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Components.Modals.Utility;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Games;

namespace SaveIt.App.UI.Components.Custom;
public partial class EditGame
{
    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required GameModel Model { get; set; }

    private void ImageChanged(ImageModel image)
    {
        Model.Image = image;
    }

    private async Task ShowLocalExecutablePickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.InitialSelectedFile), Model.GameExecutableFile! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Files },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Both},
            { nameof(LocalItemPickerModal.AllowedExtensions), new List<string>(){ ".lnk", ".exe" } },
            { nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    Model.GameExecutableFile = file;
                    await ModalLocalItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalItemPicker.ShowAsync<LocalItemPickerModal>("Select Game Executable", parameters: parameters);
    }

    private void ClearLocalExecutablePath()
    {
        Model.GameExecutableFile = null;
    }
}