using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals.Utility;
public partial class LocalItemPickerModal
{
    private readonly List<SelectedItemViewModel<LocalFileItemModel>> _items = [];
    private string? _error;

    [Parameter, EditorRequired]
    public required EventCallback<LocalFileItemModel> OnItemSelected { get; set; }

    [Parameter]
    public LocalPickerMode PickerMode { get; set; } = LocalPickerMode.Both;

    [Parameter]
    public LocalPickerMode ShowMode { get; set; } = LocalPickerMode.Both;

    [Parameter]
    public IEnumerable<string> AllowedExtensions { get; set; } = [];

    [Parameter]
    public LocalFileItemModel? InitialSelectedFile { get; set; }

    private SelectedItemViewModel<LocalFileItemModel> _selectedFile = default!;
    private bool _isLoading = false;

    protected override void OnInitialized()
    {
        try
        {
            if (InitialSelectedFile is not null)
            {
                var folder = Path.GetDirectoryName(InitialSelectedFile.FullPath);
                _selectedFile = new(new LocalFileItemModel
                {
                    Name = Path.GetFileName(folder)!,
                    Path = Directory.GetParent(folder!)?.FullName ?? "",
                    IsDirectory = true
                });
                return;
            }

            if (_selectedFile is null)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                _selectedFile = new(new LocalFileItemModel
                {
                    Name = Path.GetFileName(folder)!,
                    Path = Directory.GetParent(folder)?.FullName ?? "",
                    IsDirectory = true
                });
            }

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
            _isLoading = true;
            if (ShowMode == LocalPickerMode.Folders || ShowMode == LocalPickerMode.Both)
            {
                var directories = Directory.GetDirectories(_selectedFile.Item.FullPath);

                _items.AddRange(directories.Select(x => GetFileItem(x, _selectedFile.Item.FullPath, true)));
            }

            if (ShowMode == LocalPickerMode.Files || ShowMode == LocalPickerMode.Both)
            {
                var files = Directory.GetFiles(_selectedFile.Item.FullPath);

                var enableFiltering = AllowedExtensions.Any();

                var filteredFiles = files
                    .Where(x => !enableFiltering || AllowedExtensions.Any(y =>
                        string.Equals(y, Path.GetExtension(x), StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => GetFileItem(x, _selectedFile.Item.FullPath));

                _items.AddRange(filteredFiles);
            }
            _isLoading = false;
        }
        catch (Exception)
        {
            _items.Clear();
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private static SelectedItemViewModel<LocalFileItemModel> GetFileItem(string filePath, string parentPath,
        bool isDirectory = false)
        => new(new LocalFileItemModel()
        {
            Name = Path.GetFileName(filePath)!,
            Path = parentPath,
            IsDirectory = isDirectory
        });

    private void MoveToDirectory(SelectedItemViewModel<LocalFileItemModel> model)
    {
        if (!model.Item.IsDirectory)
        {
            return;
        }

        _selectedFile = model;
        RedrawItems();
    }

    private void ChangeToParentDirectory()
    {
        var parent = _selectedFile.Item.Path;

        if (string.IsNullOrEmpty(parent))
        {
            return;
        }

        string parentPath;
        try
        {
            parentPath = Path.GetDirectoryName(parent)!;
        }
        catch (Exception)
        {
            return;
        }

        _selectedFile = GetFileItem(parent, parentPath, true);
        RedrawItems();
    }

    private void SelectItem(SelectedItemViewModel<LocalFileItemModel> item)
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

        if (selectedItem is null && (PickerMode == LocalPickerMode.Folders || PickerMode == LocalPickerMode.Both))
        {
            await OnItemSelected.InvokeAsync(_selectedFile.Item);
            return;
        }

        if (selectedItem is null)
        {
            return;
        }

        if (selectedItem.Item.IsDirectory && PickerMode == LocalPickerMode.Files)
        {
            MoveToDirectory(selectedItem);
            return;
        }

        await OnItemSelected.InvokeAsync(selectedItem.Item);
    }

    private Task SelectAndPickAsync(SelectedItemViewModel<LocalFileItemModel> item)
    {
        SelectItem(item);
        return PickItemAsync();
    }

    public enum LocalPickerMode
    {
        Files = 1,
        Folders = 2,
        Both = 3
    }
}