using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class ItemPicker<TItem> where TItem : NamedModel
{
    [Parameter]
    public EventCallback OnPickerClicked { get; set; }

    [Parameter]
    public EventCallback OnPathCleared { get; set; }

    [Parameter]
    public TItem? SelectedFile { get; set; }

    [Parameter]
    public bool IsRequired { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public IconName Icon { get; set; } = default!;

    private async Task ShowModal()
    {
        await OnPickerClicked.InvokeAsync();
    }

    private Task ClearPath()
    {
        SelectedFile = default;
        return OnPathCleared.InvokeAsync();
    }
}