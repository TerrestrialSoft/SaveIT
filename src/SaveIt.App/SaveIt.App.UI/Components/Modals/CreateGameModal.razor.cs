using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Components.Modals;
public partial class CreateGameModal
{
    public const string Title = "Create New Game";

    [Inject]
    private IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Parameter]
    public Modal ModalLocalFolderPicker { get; set; } = default!;

    [Parameter]
    public Modal ModalAuthorizeStorage { get; set; } = default!;

    [Parameter]
    public Modal ModalCurrent { get; set; } = default!;

    [Parameter]
    public NewGame EditGame { get; set; } = default!;

    private StorageAccount _selectedStorageAccount = new();
    private List<StorageAccount> _storageAccounts = [];

    protected override async Task OnInitializedAsync()
    {
        await RefreshStorageAccountsAsync();
    }

    private async Task RefreshStorageAccountsAsync()
    {
        _storageAccounts = (await StorageAccountRepository.GetAllStorageAccounts()).ToList();
    }

    private async Task ShowLocalFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalFolderPickerModal.InitialPath), EditGame.LocalGameSavePath },
            {
                nameof(LocalFolderPickerModal.OnDirectorySelected),
                EventCallback.Factory.Create<string>(this, async (path) =>
                {
                    EditGame.LocalGameSavePath = path;
                    await ModalLocalFolderPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalFolderPicker.ShowAsync<LocalFolderPickerModal>("Select Game Save Folder", parameters: parameters);
    }

    private async Task ShowAuthorizeStorageModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>()
        {
            {
                nameof(StorageAuthorizationModal.OnClose),
                EventCallback.Factory.Create(this, async () =>
                {
                    await RefreshStorageAccountsAsync();
                    await ModalAuthorizeStorage.HideAsync();
                    await ModalLocalFolderPicker.ShowAsync();
                })
            }
        };

        await ModalAuthorizeStorage.ShowAsync<StorageAuthorizationModal>("Authorize new Cloud Storage", parameters: parameters);
    }
}