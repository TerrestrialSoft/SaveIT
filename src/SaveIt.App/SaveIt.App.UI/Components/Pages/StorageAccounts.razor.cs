using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models.StorageAccounts;

namespace SaveIt.App.UI.Components.Pages;
public partial class StorageAccounts
{
    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    private Grid<StorageAccountModel> _grid = default!;
    private List<StorageAccountModel> _storageAccounts = default!;

    private async Task<GridDataProviderResult<StorageAccountModel>> StorageAccountsDataProvider(
        GridDataProviderRequest<StorageAccountModel> request)
    {
        if (_storageAccounts is not null && _storageAccounts.Count != 0)
        {
            return await Task.FromResult(request.ApplyTo(_storageAccounts));
        }

        var accounts = await StorageAccountRepository.GetAllAsync();
        _storageAccounts = accounts.Select(x => new StorageAccountModel
        {
            Id = x.Id,
            Email = x.Email,
            Type = x.Type,
            IsAuthorized = x.IsAuthorized
        }).ToList();

        return await Task.FromResult(request.ApplyTo(_storageAccounts));
    }

    private async Task ShowAuthorizeStorageModalAsync()
    {

    }

    private async Task RenewAuthorizationAsync(StorageAccountModel account)
    {

    }

    private async Task DeleteStorageAccountAsync(StorageAccountModel account)
    {

    }
}