using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models.Game;

namespace SaveIt.App.UI.Components.Modals;
public partial class EditGameModal
{
    [Parameter, EditorRequired]
    public required CreateGameModel Model { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    public async Task ValidSubmitAsync()
    {

    }

    public Task CancelUpdateAsync()
        => ModalCurrent.HideAsync();
}