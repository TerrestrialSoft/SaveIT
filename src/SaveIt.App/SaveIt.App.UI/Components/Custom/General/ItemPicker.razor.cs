using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class ItemPicker<TItem> where TItem : NamedModel
{
    [Parameter]
    public required string Title { get; set; }

    [Parameter, EditorRequired]
    public required IconName Icon { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnPickerClicked { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnPathCleared { get; set; }

    [Parameter]
    public TItem? SelectedFile { get; set; }

    [Parameter]
    public bool IsRequired { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public int TabIndex { get; set; }

    [Parameter]
    public string? HelpText { get; set; }

    private Task ShowModalAsync()
        => OnPickerClicked.InvokeAsync();

    private Task ClearPathAsync()
    {
        SelectedFile = default;
        return OnPathCleared.InvokeAsync();
    }
}