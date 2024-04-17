using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class Help
{
    [Parameter, EditorRequired]
    public required string Text { get; set; }
}