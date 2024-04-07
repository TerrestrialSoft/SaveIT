using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals;
public partial class LocalItemPickerModal
{
    private string _currentPath = string.Empty;
    private readonly List<LocalItemModel> _items = [];
    private string? _error;

    [Parameter]
    public EventCallback<string> OnItemSelected { get; set; }

    [Parameter]
    public LocalPickerMode PickerMode { get; set; } = LocalPickerMode.Both;

    [Parameter]
    public LocalPickerMode ShowMode { get; set; } = LocalPickerMode.Both;

    [Parameter]
    public IEnumerable<string> AllowedExtensions { get; set; } = [];

    public enum LocalPickerMode
    {
        Files = 1,
        Folders = 2,
        Both = 3
    }

    [Parameter]
    public string InitialPath { get; set; } = "";

    protected override void OnInitialized()
    {
        try
        {
            _currentPath = string.IsNullOrEmpty(InitialPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                : Path.GetDirectoryName(InitialPath)!;

            RedrawItems();
        }
        catch (Exception)
        {
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private void RedrawItems()
    {
        _error = null;
        _items.Clear();
        try
        {
            if (ShowMode == LocalPickerMode.Folders || ShowMode == LocalPickerMode.Both)
            {
                var directories = Directory.GetDirectories(_currentPath);

                _items.AddRange(directories.Select(x => new LocalItemModel
                {
                    Name = Path.GetFileName(x)!,
                    Path = x,
                    IsDirectory = true
                }));
            }

            if (ShowMode == LocalPickerMode.Files || ShowMode == LocalPickerMode.Both)
            {
                var files = Directory.GetFiles(_currentPath);
                var filteredFiles = files
                    .Where(x => AllowedExtensions.Any(y =>
                        string.Equals(y, Path.GetExtension(x),StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => new LocalItemModel
                    {
                        Name = Path.GetFileName(x)!,
                        Path = x
                    });

                _items.AddRange(filteredFiles);
            }
        }
        catch (Exception)
        {
            _items.Clear();
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private void MoveToDirectory(LocalItemModel model)
    {
        if (!model.IsDirectory)
        {
            return;
        }

        _currentPath = model.Path;
        RedrawItems();
    }

    private void ChangeToParentDirectory()
    {
        var parent = Directory.GetParent(_currentPath);

        if (parent is null)
        {
            return;
        }

        _currentPath = parent.FullName;
        RedrawItems();
    }

    private void SelectItem(LocalItemModel item)
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        if (selectedItem is not null)
        {
            selectedItem.IsSelected = false;
        }

        item.IsSelected = true;
    }

    private async Task PickItemAsync()
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        if(selectedItem is null && (PickerMode == LocalPickerMode.Folders || PickerMode == LocalPickerMode.Both))
        {
            await OnItemSelected.InvokeAsync(_currentPath);
            return;
        }

        if(selectedItem is null)
        {
            return;
        }

        if(selectedItem.IsDirectory && PickerMode == LocalPickerMode.Files)
        {
            MoveToDirectory(selectedItem);
            return;
        }

        await OnItemSelected.InvokeAsync(selectedItem.Path);
    }

    private Task SelectAndPickAsync(LocalItemModel item)
    {
        SelectItem(item);
        return PickItemAsync();
    }

    private void RemovePath()
    {
        _currentPath = "";
    }
}