using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals;
public partial class LocalItemPickerModal
{
    private readonly List<SelectedItemViewModel<LocalFileItemModel>> _items = [];
    private string? _error;

    [Parameter]
    public EventCallback<LocalFileItemModel> OnItemSelected { get; set; }

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

    private SelectedItemViewModel<LocalFileItemModel> _selectedFile = default!;

    [Parameter]
    public LocalFileItemModel? SelectedFile { get; set; }

    protected override void OnInitialized()
    {
        if (SelectedFile is not null)
        {
            _selectedFile = new(SelectedFile);
        }

        try
        {
            if(_selectedFile is null)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                _selectedFile = new(new LocalFileItemModel
                {
                    Name = Path.GetFileName(folder)!,
                    Path = Directory.GetParent(folder)?.FullName ?? "",
                    IsDirectory = true
                });
            }
            else
            {
                _selectedFile = new(new LocalFileItemModel
                {
                    Name = Path.GetDirectoryName(_selectedFile.Item.Path)!,
                    Path = Path.GetDirectoryName(_selectedFile.Item.Path)!,
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
            if (ShowMode == LocalPickerMode.Folders || ShowMode == LocalPickerMode.Both)
            {
                var directories = Directory.GetDirectories(_selectedFile.Item.FullPath);

                _items.AddRange(directories.Select(x => new SelectedItemViewModel<LocalFileItemModel>(new()
                {
                    Name = Path.GetFileName(x)!,
                    Path = _selectedFile.Item.FullPath,
                    IsDirectory = true
                })));
            }

            if (ShowMode == LocalPickerMode.Files || ShowMode == LocalPickerMode.Both)
            {
                var files = Directory.GetFiles(_selectedFile.Item.FullPath);

                var enableFiltering = AllowedExtensions.Any();

                var filteredFiles = files
                    .Where(x => !enableFiltering || AllowedExtensions.Any(y =>
                        string.Equals(y, Path.GetExtension(x),StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => new SelectedItemViewModel<LocalFileItemModel>(new()
                    {
                        Name = Path.GetFileName(x)!,
                        Path = _selectedFile.Item.FullPath
                    }));

                _items.AddRange(filteredFiles);
            }
        }
        catch (Exception)
        {
            _items.Clear();
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

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
        var parent = Directory.GetParent(_selectedFile.Item.Path!);

        if (parent is null)
        {
            return;
        }

        _selectedFile.Item.Name = parent.FullName;
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

        if(selectedItem is null && (PickerMode == LocalPickerMode.Folders || PickerMode == LocalPickerMode.Both))
        {
            await OnItemSelected.InvokeAsync(_selectedFile.Item);
            return;
        }

        if(selectedItem is null)
        {
            return;
        }

        if(selectedItem.Item.IsDirectory && PickerMode == LocalPickerMode.Files)
        {
            MoveToDirectory(selectedItem);
            return;
        }

        await OnItemSelected.InvokeAsync(_selectedFile.Item);
    }

    private Task SelectAndPickAsync(SelectedItemViewModel<LocalFileItemModel> item)
    {
        SelectItem(item);
        return PickItemAsync();
    }
}