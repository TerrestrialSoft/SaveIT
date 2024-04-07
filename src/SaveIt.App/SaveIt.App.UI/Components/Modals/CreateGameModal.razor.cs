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
    public NewGameModel EditGame { get; set; } = default!;

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
            { nameof(LocalItemPickerModal.InitialPath), EditGame.LocalGameSavePath },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Folders },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Folders},
            {
                nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<string>(this, async (path) =>
                {
                    EditGame.LocalGameSavePath = path;
                    await ModalLocalFolderPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalFolderPicker.ShowAsync<LocalItemPickerModal>("Select Game Save Folder", parameters: parameters);
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

    private void ImageUploaded(ImageModel image)
    {
        EditGame.Image = image;
    }

    private async Task ShowLocalExecutablePickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.InitialPath), EditGame.LocalExecutablePath! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Files },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Both},
            { nameof(LocalItemPickerModal.AllowedExtensions), new List<string>(){ ".exe" } },
            {
                nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<string>(this, async (path) =>
                {
                    EditGame.LocalExecutablePath = path;
                    await ModalLocalFolderPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalFolderPicker.ShowAsync<LocalItemPickerModal>("Select Game Executable", parameters: parameters);
    }

    private async Task ShowRemoteFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            //{ nameof(LocalItemPickerModal.InitialPath), EditGame.LocalExecutablePath! },
            //{ nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Files },
            //{ nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Both},
            //{ nameof(LocalItemPickerModal.AllowedExtensions), new List<string>(){ ".exe" } },
            //{
            //    nameof(LocalItemPickerModal.OnItemSelected),
            //    EventCallback.Factory.Create<string>(this, async (path) =>
            //    {
            //        EditGame.LocalExecutablePath = path;
            //        await ModalLocalFolderPicker.HideAsync();
            //        await ModalCurrent.ShowAsync();
            //    })
            //},
        };

        await ModalLocalFolderPicker.ShowAsync<RemoteRepositoryPickerModal>("Select Game Storage", parameters: parameters);
    }

    private void ClearLocalExecutablePath()
    {
        EditGame.LocalExecutablePath = null;
    }

    private void ClearLocalGameSavePath()
    {
        EditGame.LocalGameSavePath = "";
    }

    private void ClearRemoteGameSavePath()
    {
        EditGame.RemoteGameSavePath = "";
    }
}