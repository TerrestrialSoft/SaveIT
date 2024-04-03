using Microsoft.AspNetCore.Components;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals;

// TODO: Needs refactoring breaking layers of abstraction
public partial class LocalFolderPickerModal
{
    private string _currentPath = string.Empty;
    private List<DirectoryModel> _directories = [];
    private string? _error;

    [Parameter]
    public EventCallback<string> OnDirectorySelected { get; set; }

    [Parameter]
    public string InitialPath { get; set; } = "";

    protected override void OnInitialized()
    {
        try
        {
            _currentPath = string.IsNullOrEmpty(InitialPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                : InitialPath;

            RedrawDirectories();
        }
        catch (Exception)
        {
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private void RedrawDirectories()
    {
        _error = null;
        try
        {
            var directories = Directory.GetDirectories(_currentPath);

            _directories = directories.Select(x => new DirectoryModel
            {
                Name = Path.GetFileName(x)!,
                Path = x
            }).ToList();
        }
        catch (Exception)
        {
            _directories.Clear();
            _error = "Application has no permission to view the contents of this folder.";
        }
    }

    private void MoveToDirectory(DirectoryModel model)
    {
        _currentPath = model.Path;
        RedrawDirectories();
    }

    private void ChangeToParentDirectory()
    {
        var parent = Directory.GetParent(_currentPath);

        if (parent is null)
        {
            return;
        }

        _currentPath = parent.FullName;
        RedrawDirectories();
    }

    private void SelectDirectory(DirectoryModel directory)
    {
        var selectedDirectory = _directories.FirstOrDefault(x => x.IsSelected);

        if (selectedDirectory is not null)
        {
            selectedDirectory.IsSelected = false;
        }

        directory.IsSelected = true;
    }

    private async Task PickDirectoryAsync()
    {
        var selectedDirectory = _directories.FirstOrDefault(x => x.IsSelected);

        string path = selectedDirectory is null
            ? _currentPath
            : selectedDirectory.Path;

        await OnDirectorySelected.InvokeAsync(path);
    }
}
