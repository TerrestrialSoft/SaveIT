using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Repositories;
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

    [Parameter]
    public Guid SelectedStorageAccountId { get; set; }

    [Parameter]
    public EventCallback<RemoteFileItemModel> OnItemSelected { get; set; }

    [Parameter]
    public RemoteFileItemModel? SelectedItem { get; set; }

    private SelectedItemViewModel<RemoteFileItemModel> _selectedItem = default!;

    protected override async Task OnInitializedAsync()
    {
        _selectedItem = SelectedItem is not null
            ? new(SelectedItem)
            : new(new RemoteFileItemModel { Name = "root", IsDirectory = true, ParentId = "root", Id = "root" });

        string? error = null;

        try
        {
            await RedrawItemsAsync();
        }
        catch (Exception)
        {
            error = "Application has no permission to view the contents of this folder.";
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
            var itemsResult = await StorageService.GetFilesAsync(SelectedStorageAccountId, _selectedItem.Item.ParentId);

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

            _items.AddRange(itemsResult.Value.Select(x => new SelectedItemViewModel<RemoteFileItemModel>(new ()
            {
                Name = x.Name,
                IsDirectory = x.FileType == Domain.Models.FileItemType.Folder,
                ParentId = x.ParentId,
                Id = x.Id!
            })));
        }
        catch (Exception)
        {
            error = "There was a problem during retrieving data from the cloud storage.";
        }

        FinishLoadingWithResult(error);
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
}