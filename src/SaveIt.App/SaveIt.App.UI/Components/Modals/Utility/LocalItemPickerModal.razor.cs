using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.UI.Xaml.Controls.Primitives;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals.Utility;
public partial class LocalItemPickerModal
{
    [Inject]
    private ToastService ToastService { get; set; } = default!;

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

    private readonly List<SelectedItemViewModel<LocalFileItemModel>> _items = [];
    private readonly LocalFilePathModel _localFilePathModel = new();
    private SelectedItemViewModel<LocalFileItemModel> _selectedFile = default!;
    private bool _isLoading = false;
    private string? _error;
    private string? _information;
    private bool _showingDrives = false;


    protected override void OnInitialized()
    {
        try
        {
            if (InitialSelectedFile is not null)
            {
                _selectedFile = new(InitialSelectedFile, !InitialSelectedFile.IsDirectory);
            }

            if (_selectedFile is null)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                _selectedFile = GetFolderItem(folder);
            }

            RedrawItems(true);
        }
        catch (Exception)
        {
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private static SelectedItemViewModel<LocalFileItemModel> GetFolderItem(string path)
    {
        var parent = Directory.GetParent(path);

        return new(new LocalFileItemModel
        {
            Name = parent is not null
                ? Path.GetFileName(path)!
                : Path.TrimEndingDirectorySeparator(new DirectoryInfo(path).Name),
            Path = parent?.FullName ?? "",
            IsDirectory = true
        });
    }

    private static SelectedItemViewModel<LocalFileItemModel> GetDriveItem(string driveName)
    {
        var driveInfo = driveName.TrimEnd(Path.DirectorySeparatorChar);
        return new(new LocalFileItemModel
        {
            Name = driveInfo,
            Path = driveName,
            IsDrive = true
        });
    }

    private void OnPathKeyPressed(KeyboardEventArgs e)
    {
        if (e.Key != "Enter" && e.Key != "NumpadEnter")
        {
            return;
        }

        UpdateSelectedPath();
    }

    private void UpdateSelectedPath()
    {
        if (_selectedFile.Item.DirectoryPath == _localFilePathModel.Path)
        {
            return;
        }

        if (Directory.Exists(_localFilePathModel.Path))
        {
            _selectedFile = GetFolderItem(_localFilePathModel.Path);
        }
        else if (File.Exists(_localFilePathModel.Path))
        {
            _selectedFile = GetFileItem(_localFilePathModel.Path);
        }
        else
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Invalid path"));
            _localFilePathModel.Path = _selectedFile.Item.DirectoryPath;
            return;
        }

        RedrawItems();
    }

    private void RedrawItems(bool firstRedraw = false)
    {
        _information = null;
        _localFilePathModel.Path = _selectedFile.Item.DirectoryPath;
        _error = null;
        _items.Clear();
        try
        {
            _isLoading = true;

            if(_showingDrives)
            {
                try
                {
                    var drives = DriveInfo.GetDrives();
                    _items.AddRange(drives.Select(x => GetDriveItem(x.Name)));
                }
                catch (Exception)
                {
                    _error = "Application has no permission to view the contents of this folder.";
                    return;
                }

                _isLoading = false;

                return;
            }

            var info = "No items found in this folder.";
            if (ShowMode == LocalPickerMode.Folders || ShowMode == LocalPickerMode.Both)
            {
                var directories = Directory.GetDirectories(_selectedFile.Item.DirectoryPath);
                _items.AddRange(directories.Select(GetFolderItem));
            }

            if (ShowMode == LocalPickerMode.Files || ShowMode == LocalPickerMode.Both)
            {
                var files = Directory.GetFiles(_selectedFile.Item.DirectoryPath);
                var enableFiltering = AllowedExtensions.Any();

                var filteredFiles = files
                    .Where(x => !enableFiltering || AllowedExtensions.Any(y =>
                        string.Equals(y, Path.GetExtension(x), StringComparison.InvariantCultureIgnoreCase)))
                    .Select(x => GetFileItem(x));

                if (files.Length > 0 && !filteredFiles.Any())
                {
                    info = "No items found with the specified filter.";
                }

                _items.AddRange(filteredFiles);

                if (firstRedraw && files.Length > 0)
                {
                    var index = _items.FindIndex(x => x.Item.FullPath == _selectedFile.Item.FullPath);
                    if (index != -1)
                    {
                        _items[index] = _selectedFile;
                    }
                }
            }
            _isLoading = false;

            if (_items.Count == 0)
            {
                _information = info;
            }
        }
        catch (Exception)
        {
            _items.Clear();
            _isLoading = false;
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private static SelectedItemViewModel<LocalFileItemModel> GetFileItem(string filePath)
        => new(new LocalFileItemModel()
        {
            Name = Path.GetFileName(filePath)!,
            Path = Path.GetDirectoryName(filePath)!
        });

    private void MoveToDirectory(SelectedItemViewModel<LocalFileItemModel> model)
    {
        if (!model.Item.IsDirectory && !model.Item.IsDrive)
        {
            return;
        }

        if (_showingDrives)
        {
            _showingDrives = false;
        }
        else
        {
            _selectedFile = model;
        }

        RedrawItems();
    }

    private void ChangeToParentDirectory()
    {
        var parent = _selectedFile.Item.DirectoryPath;

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

        if (parentPath == null)
        {
            _showingDrives = true;
        }
        else
        {
            _selectedFile = GetFolderItem(parentPath);
        }
        
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

    private bool CanPickItem()
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        return _isLoading
            || (_showingDrives && selectedItem is null)
            || (string.IsNullOrEmpty(_selectedFile.Item.Path) && selectedItem is null);
    }

    private async Task PickItemAsync()
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        if(selectedItem is null && _showingDrives)
        {
            return;
        }

        if(selectedItem is not null && selectedItem.Item.IsDrive)
        {
            MoveToDirectory(selectedItem);
            return;
        }

        if (selectedItem is null && (PickerMode == LocalPickerMode.Folders || PickerMode == LocalPickerMode.Both))
        {
            if(string.IsNullOrEmpty(_selectedFile.Item.Path))
            {
                return;
            }

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
