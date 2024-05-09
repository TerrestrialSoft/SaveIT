using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals.Utility;
using SaveIt.App.UI.Models.StorageAccounts;

namespace SaveIt.App.UI.Components.Pages;
public partial class StorageAccounts
{
    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Inject]
    private PreloadService PreloadService { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    private Grid<StorageAccountModel> _grid = default!;
    private List<StorageAccountModel> _storageAccounts = default!;

    private Modal _authorizeStorageModal = default!;
    private ConfirmDialog _confirmDialog = default!;

    private async Task<GridDataProviderResult<StorageAccountModel>> StorageAccountsDataProvider(
        GridDataProviderRequest<StorageAccountModel> request)
    {
        if (_storageAccounts is not null && _storageAccounts.Count != 0)
        {
            return await Task.FromResult(request.ApplyTo(_storageAccounts));
        }

        var accounts = await StorageAccountRepository.GetAllWithChildrenAsync();
        _storageAccounts = accounts.Select(x => new StorageAccountModel
        {
            Id = x.Id,
            Email = x.Email,
            Type = x.Type,
            IsAuthorized = x.IsAuthorized,
            GameSavesCount = x.GameSaves!.Count
        }).ToList();

        return await Task.FromResult(request.ApplyTo(_storageAccounts));
    }

    private async Task ShowAuthorizeStorageModalAsync()
    {
        var parameters = new Dictionary<string, object>()
        {
            { nameof(StorageAuthorizationModal.OnClose), EventCallback.Factory.Create(this, async () =>
                {
                    _storageAccounts.Clear();
                    await _grid.RefreshDataAsync();
                    await _authorizeStorageModal.HideAsync();
                })
            }
        };

        await _authorizeStorageModal.ShowAsync<StorageAuthorizationModal>("Authorize new Cloud Storage",
            parameters: parameters);
    }

    private async Task RenewAuthorizationAsync(StorageAccountModel account)
    {
        if(account.IsAuthorized)
        {
            return;
        }

        var parameters = new Dictionary<string, object>()
        {
            { nameof(StorageAuthorizationModal.StorageAccountType), account.Type },
            { nameof(StorageAuthorizationModal.OnClose), EventCallback.Factory.Create(this, async () =>
                {
                    _storageAccounts.Clear();
                    await _grid.RefreshDataAsync();
                    await _authorizeStorageModal.HideAsync();
                })
            }
        };

        await _authorizeStorageModal.ShowAsync<StorageAuthorizationModal>("Authorize new Cloud Storage",
            parameters: parameters);
    }

    private async Task DeleteStorageAccountAsync(StorageAccountModel account)
    {
        if(account.GameSavesCount != 0)
        {
            return;
        }

        var dialogResult = await _confirmDialog.ShowDeleteDialogAsync($"Delete {account.Email} ({account.Type})",
            "Are you sure you want to delete this storage account?",
            "This action cannot be undone.");

        if(!dialogResult)
        {
            return;
        }

        PreloadService.Show();

        await StorageAccountRepository.DeleteAsync(account.Id);
        _storageAccounts.Remove(account);
        PreloadService.Hide();

        ToastService.Notify(new ToastMessage(ToastType.Success, "Successfully deleted Storage Account."));


        await _grid.RefreshDataAsync();
    }
}