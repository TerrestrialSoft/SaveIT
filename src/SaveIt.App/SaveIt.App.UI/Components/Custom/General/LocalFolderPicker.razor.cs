using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class LocalFolderPicker
{
    [Parameter]
    public EventCallback OnPickerClicked { get; set; }

    [Parameter]
    public string Path { get; set; } = "";

    private async Task ShowModal()
    {
        await OnPickerClicked.InvokeAsync();
    }
}
