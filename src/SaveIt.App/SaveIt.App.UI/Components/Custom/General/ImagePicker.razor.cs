using Microsoft.AspNetCore.Components.Forms;

namespace SaveIt.App.UI.Components.Custom.General;
public partial class ImagePicker
{
    private string? _base64;
    private const long _maxFileSize = 5 * 1024 * 1024;
    private string? _error = null;

    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        using MemoryStream memoryStream = new MemoryStream();

        if (e.File.Size > _maxFileSize)
        {
            _error = "File size is too large";
            return;
        }

        await e.File.OpenReadStream(_maxFileSize)
            .CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();
        _base64 = "data:image/png;base64," + Convert.ToBase64String(bytes);
    }
}
