using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class ImagePicker
{
    private const long _maxFileSize = 5 * 1024 * 1024;
    private string? _error = null;

    [Parameter]
    public EventCallback<ImageModel> OnImageUploaded { get; set; }

    [Parameter]
    public ImageModel? ImageSrc { get; set; }

    [Parameter]
    public int TabIndex { get; set; }

    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        _error = null;
        using MemoryStream memoryStream = new MemoryStream();

        if (e.File.Size > _maxFileSize)
        {
            _error = "File size is too large";
            return;
        }

        await e.File.OpenReadStream(_maxFileSize)
            .CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();

        string data = "data:image/png;base64," + Convert.ToBase64String(bytes);

        ImageModel imageModel = new(e.File.Name, data);
        await OnImageUploaded.InvokeAsync(imageModel);
    }

    private void RemoveImage()
    {
        ImageSrc = null;
    }
}
