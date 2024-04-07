using BlazorBootstrap;
using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class LocalItemPicker
{
    [Parameter]
    public EventCallback OnPickerClicked { get; set; }

    [Parameter]
    public EventCallback OnPathCleared { get; set; }

    [Parameter]
    public string SelectedPath { get; set; } = "";

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
        SelectedPath = "";
        return OnPathCleared.InvokeAsync();
    }
}