using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.UI.Components.Modals.Utility;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Components.Custom;
public partial class EditGameSave
{
    [Inject]
    public IStorageAccountRepository StorageAccountRepository { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Modal ModalLocalItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalRemoteItemPicker { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalAuthorizeStorage { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required GameSaveModel Model { get; set; }

    [Parameter]
    public GameSave? GameSave { get; set; }

    private ConfirmDialog confirmDialog = default!;
    private List<StorageAccount> _storageAccounts = [];

    private async Task ShowLocalFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalItemPickerModal.InitialSelectedFile), Model.LocalGameSaveFile! },
            { nameof(LocalItemPickerModal.PickerMode), LocalItemPickerModal.LocalPickerMode.Folders },
            { nameof(LocalItemPickerModal.ShowMode), LocalItemPickerModal.LocalPickerMode.Folders},
            { nameof(LocalItemPickerModal.OnItemSelected),
                EventCallback.Factory.Create<LocalFileItemModel>(this, async (file) =>
                {
                    Model.LocalGameSaveFile = file;
                    if (GameSave is not null)
                    {
                        GameSave.LocalGameSavePath = file.Path;
                    }
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
                    await ModalAuthorizeStorage.HideAsync();
                    await ModalCurrent.ShowAsync();
                    await RefreshStorageAccountsAsync();
                    Model.StorageAccountId = _storageAccounts[^1].Id;
                    Model.RemoteGameSaveFile = null;
                })
            }
        };

        await ModalAuthorizeStorage.ShowAsync<StorageAuthorizationModal>("Authorize new Cloud Storage", parameters: parameters);
    }

    private async Task ShowRemoteFolderPickerModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(RemoteRepositoryPickerModal.InitialSelectedItem), Model.RemoteGameSaveFile! },
            { nameof(RemoteRepositoryPickerModal.SelectedStorageAccountId), Model.StorageAccountId! },
            { nameof(RemoteRepositoryPickerModal.OnItemSelected),
                EventCallback.Factory.Create<RemoteFileItemModel>(this, async (file) =>
                {
                    Model.RemoteGameSaveFile = file;

                    GameSave ??= new GameSave();

                    GameSave.RemoteLocationId = file.Id;
                    GameSave.RemoteLocationName = file.Name;

                    await ModalRemoteItemPicker.HideAsync();
                    await ModalCurrent.ShowAsync();
                })
            },
        };

        await ModalRemoteItemPicker.ShowAsync<RemoteRepositoryPickerModal>("Select Game Storage", parameters: parameters);
    }

    private void ClearLocalGameSavePath()
    {
        Model.LocalGameSaveFile = null;
    }

    private void ClearRemoteGameSavePath()
    {
        Model.RemoteGameSaveFile = null;
    }

    private async Task ChangeStorageAccountAsync(Guid? id)
    {
        if (Model.RemoteGameSaveFile is not null)
        {
            var options = new ConfirmDialogOptions
            {
                YesButtonText = "Cancel",
                YesButtonColor = ButtonColor.Primary,
                NoButtonText = "Delete",
                NoButtonColor = ButtonColor.Secondary,
                Size = DialogSize.Large,
                IsVerticallyCentered = true,
                DialogCssClass = "fs-5",
                Dismissable = false
            };

            var wasCancelled = await confirmDialog.ShowAsync(
                title: $"Change storage provider",
                message1: $"You have already selected Remote File." +
                $"Changing Storage Provider will remove previously selected Remote File.",
                message2: "Do you wish to continue?",
                options);

            if (wasCancelled)
            {
                return;
            }
        }

        Model.StorageAccountId = id;
        Model.RemoteGameSaveFile = null;
    }

    protected override async Task OnInitializedAsync()
    {
        await RefreshStorageAccountsAsync();
        TrySetStorageAccount();
    }

    private async Task RefreshStorageAccountsAsync()
    {
        _storageAccounts = (await StorageAccountRepository.GetAllAsync()).ToList();
    }

    private void TrySetStorageAccount()
    {
        if (_storageAccounts.Count != 0 && Model.StorageAccountId is null)
        {
            Model.StorageAccountId = _storageAccounts[0].Id;
        }
    }
}