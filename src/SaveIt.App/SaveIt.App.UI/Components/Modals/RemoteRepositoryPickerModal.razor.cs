using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Extensions;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals;
public partial class RemoteRepositoryPickerModal
{
    private readonly List<SelectedItemViewModel<RemoteFileItemModel>> _items = [];
    private string? _error;
    private bool _isLoading = true;

    [Inject]
    private IExternalStorageService StorageService { get; set; } = default!;

    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Guid SelectedStorageAccountId { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<RemoteFileItemModel> OnItemSelected { get; set; }

    [Parameter]
    public RemoteFileItemModel? InitialSelectedItem { get; set; }

    private SelectedItemViewModel<RemoteFileItemModel> _selectedItem = default!;

    public RemoteRepositoryPickerModal()
    {
        if (InitialSelectedItem is not null)
        {
            _selectedItem = new(InitialSelectedItem);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        string? error = null;
        FileItemModel folder;

        try
        {
            var fileId = InitialSelectedItem?.Id ?? RemoteFileItemModel.DefaultId;
            var folderResult = await StorageService.GetFileAsync(SelectedStorageAccountId, fileId);

            if (folderResult.IsFailed)
            {
                error = folderResult.Errors[0].Message;
                FinishLoadingWithResult(error);
                return;
            }

            folder = folderResult.Value;
        }
        catch (Exception)
        {
            error = "There was a problem during retrieving data from the cloud storage.";
            FinishLoadingWithResult(error);
            return;
        }

        _selectedItem = GetSelectRemoteFile(folder);

        try
        {
            await RedrawItemsAsync();
        }
        catch (Exception)
        {
            error = "There was a problem during retrieving data from the cloud storage.";
        }
        FinishLoadingWithResult(error);
    }

    private async Task RedrawItemsAsync()
    {
        _error = null;
        _items.Clear();
        string? error = null;
        try
        {
            var itemsResult = await StorageService.GetFilesAsync(SelectedStorageAccountId, _selectedItem.Item.Id);

            if (itemsResult.IsFailed)
            {
                if(itemsResult.HasError<AuthError>())
                {
                    await StorageAccountRepository.DeactiveAccountAsync(SelectedStorageAccountId);
                    FinishLoadingWithResult("Unable to refresh token. Storage account was deactivated");
                    return;
                }

                FinishLoadingWithResult(itemsResult.Errors[0].Message);
                return;
            }

            _items.AddRange(itemsResult.Value.Select(GetSelectRemoteFile));
        }
        catch (Exception)
        {
            error = "There was a problem during retrieving data from the cloud storage.";
        }

        FinishLoadingWithResult(error);
    }

    private static SelectedItemViewModel<RemoteFileItemModel> GetSelectRemoteFile(FileItemModel item)
        => new(item.ToRemoteFileItemModel());

    private void StartLoading()
    {
        _isLoading = true;
    }

    private void FinishLoadingWithResult(string? error = null)
    {
        _isLoading = false;

        if(error is not null)
        {
            _error = error;
            _items.Clear();
        }
    }

    private void SelectItem(SelectedItemViewModel<RemoteFileItemModel> item)
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        if (selectedItem is not null)
        {
            selectedItem.IsSelected = false;
        }

        item.IsSelected = true;
    }

    private Task MoveToItem(SelectedItemViewModel<RemoteFileItemModel> item)
    {
        _selectedItem = item;
        StartLoading();
        return RedrawItemsAsync();
    }

    private Task PickItemAsync()
    {
        var selectedItem = _items.Find(x => x.IsSelected);

        var resultItem = selectedItem is null
            ? _selectedItem.Item
            : selectedItem.Item;

        return OnItemSelected.InvokeAsync(resultItem);
    }

    private async Task ChangeToParentDirectory()
    {
        StartLoading();
        var fileResult = await StorageService.GetFileAsync(SelectedStorageAccountId, _selectedItem.Item.ParentId);

        if (fileResult.IsFailed)
        {
            FinishLoadingWithResult(fileResult.Errors[0].Message);
            return;
        }

        _selectedItem = GetSelectRemoteFile(fileResult.Value);
        await RedrawItemsAsync();
    }

    private async Task CreateRepositoryAsync()
    {
        StartLoading();
        var result = await StorageService.CreateFolderAsync(SelectedStorageAccountId, "SaveIt", _selectedItem.Item.Id);

        if (result.IsFailed)
        {
            FinishLoadingWithResult(result.Errors[0].Message);
            return;
        }

        await RedrawItemsAsync();
    }

    private async Task DeleteFileAsync()
    {
        if(_selectedItem.Item.ParentId == RemoteFileItemModel.DefaultId)
        {
            return;
        }

        StartLoading();
        var result = await StorageService.DeleteFileAsync(SelectedStorageAccountId, _selectedItem.Item.Id);

        if (result.IsFailed)
        {
            FinishLoadingWithResult(result.Errors[0].Message);
            return;
        }

        await RedrawItemsAsync();
    }
}