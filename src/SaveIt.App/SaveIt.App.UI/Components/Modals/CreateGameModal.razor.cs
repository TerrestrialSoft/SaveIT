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
    public Modal ModalLocalItemPicker { get; set; } = default!;

    [Parameter]
    public Modal ModalRemoteItemPicker { get; set; } = default!;

    [Parameter]
    public Modal ModalAuthorizeStorage { get; set; } = default!;

    [Parameter]
    public Modal ModalCurrent { get; set; } = default!;

    [Parameter]
    public NewGameModel EditGame { get; set; } = default!;

    private List<StorageAccount> _storageAccounts = [];

    protected override async Task OnInitializedAsync()
    {
        await RefreshStorageAccountsAsync();
    }

    private async Task RefreshStorageAccountsAsync()
    {
        _storageAccounts = (await StorageAccountRepository.GetAllStorageAccounts()).ToList();
        EditGame.StorageAccountId ??= _storageAccounts.FirstOrDefault()?.Id;
    }

    private async Task ShowLocalFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.SelectedFile), EditGame.LocalGameSaveFile! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Folders },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Folders},
            { nameof(LocalItemPickerModal.OnItemSelected), EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    EditGame.LocalGameSaveFile = file;
                    await ModalLocalItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalItemPicker.ShowAsync<LocalItemPickerModal>("Select Game Save Folder", parameters: parameters);
    }

    private async Task ShowAuthorizeStorageModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>()
        {
            { nameof(StorageAuthorizationModal.OnClose), EventCallback.Factory.Create(this, async () =>
                {
                    await RefreshStorageAccountsAsync();
                    await ModalAuthorizeStorage.HideAsync();
                    await ModalLocalItemPicker.ShowAsync();
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
            { nameof(LocalItemPickerModal.SelectedFile), EditGame.LocalExecutableFile! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Files },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Both},
            { nameof(LocalItemPickerModal.AllowedExtensions), new List<string>(){ ".exe" } },
            { nameof(LocalItemPickerModal.OnItemSelected), EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    EditGame.LocalExecutableFile = file;
                    await ModalLocalItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalLocalItemPicker.ShowAsync<LocalItemPickerModal>("Select Game Executable", parameters: parameters);
    }

    private async Task ShowRemoteFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(RemoteRepositoryPickerModal.SelectedItem), EditGame.RemoteGameSaveFile! },
            { nameof(RemoteRepositoryPickerModal.SelectedStorageAccountId), EditGame.StorageAccountId! },
            { nameof(RemoteRepositoryPickerModal.OnItemSelected), EventCallback.Factory.Create<RemoteFileItemModel>(this, async (file) =>
                {
                    EditGame.RemoteGameSaveFile = file;
                    await ModalRemoteItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalRemoteItemPicker.ShowAsync<RemoteRepositoryPickerModal>("Select Game Storage", parameters: parameters);
    }

    private void ClearLocalExecutablePath()
    {
        EditGame.LocalExecutableFile = null;
    }

    private void ClearLocalGameSavePath()
    {
        EditGame.LocalGameSaveFile = null;
    }

    private void ClearRemoteGameSavePath()
    {
        EditGame.RemoteGameSaveFile = null;
    }
}