using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class ImagePicker
{
    private const long _maxFileSize = 5 * 1024 * 1024;
    private string? _error = null;

    [Parameter, EditorRequired]
    public required EventCallback<ImageModel?> OnImageChanged { get; set; }

    [Parameter]
    public ImageModel? ImageSource { get; set; }

    [Parameter]
    public string? HelpText { get; set; }

    [Parameter]
    public int TabIndex { get; set; }

    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        _error = null;
        using MemoryStream memoryStream = new MemoryStream();

        if (e.File.Size > _maxFileSize)
        {
            _error = "File size is too large. Maximum size is 5 MB.";
            return;
        }

        using var readStream = e.File.OpenReadStream(_maxFileSize);
        await readStream.CopyToAsync(memoryStream);

        var content = Convert.ToBase64String(memoryStream.ToArray());
        var data = GetDataUri(e.File.ContentType, content);

        ImageModel imageModel = new(e.File.Name, data);
        await OnImageChanged.InvokeAsync(imageModel);
    }

    private static string GetDataUri(string contentType, string content)
        => $"data:{contentType};base64,{content}";

    private async Task RemoveImage()
    {
        ImageSource = null;
        await OnImageChanged.InvokeAsync(null);
    }
}
