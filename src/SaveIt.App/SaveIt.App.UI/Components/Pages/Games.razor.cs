using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;
using SaveIt.App.UI.Components.Modals;

namespace SaveIt.App.UI.Components.Pages;
public partial class Games
{
    private Modal _createNewGameModal = default!;
    private Modal _localFolderPickerModal = default!;
    private Modal _authorizeStorageModal = default!;
    private string _selectedLocalFolderPath = "";


    private List<Game> _allGames = [];
    private List<Game> _filteredGames = [];
    private string _searchText = "";
    private Game? _selectedGame = new();
    private StorageAccount _selectedStorageAccount = new();
    private List<StorageAccount> _storageAccounts = [];

    protected override async Task OnInitializedAsync()
    {
        await RefreshStorageAccountsAsync();
        _allGames = (await gameRepository.GetAllGamesAsync()).ToList();
        _filteredGames = _allGames.ToList();
    }

    private void UpdateGames(string searchText)
        => _filteredGames = _allGames.Where(g => g.Name.Contains(searchText)).ToList();

    private void GameCardClicked(Game g)
        => _selectedGame = g;

    private Task ShowCreateNewGameModal()
        => _createNewGameModal.ShowAsync();

    private async Task ShowLocalFolderPickerModal()
    {
        await _createNewGameModal.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(LocalFolderPickerModal.InitialPath), _selectedLocalFolderPath },
            {
                nameof(LocalFolderPickerModal.OnDirectorySelected),
                EventCallback.Factory.Create<string>(this, async (path) =>
                {
                    _selectedLocalFolderPath = path;
                    await _localFolderPickerModal.HideAsync();
                    await _createNewGameModal.ShowAsync();
                })
            },
        };

        await _localFolderPickerModal.ShowAsync<LocalFolderPickerModal>("Select Game Save Folder", parameters: parameters);
    }

    private async Task ShowAuthorizeStorageModal()
    {
        await _createNewGameModal.HideAsync();
        var parameters = new Dictionary<string, object>()
        {
            {
                nameof(StorageAuthorizationModal.OnClose),
                EventCallback.Factory.Create(this, async () =>
                {
                    await RefreshStorageAccountsAsync();
                    await _authorizeStorageModal.HideAsync();
                    await _createNewGameModal.ShowAsync();
                })
            }
        };

        await _authorizeStorageModal.ShowAsync<StorageAuthorizationModal>("Authorize new Cloud Storage", parameters: parameters);
    }

    private async Task RefreshStorageAccountsAsync()
    {
        _storageAccounts = (await storageAccountRepository.GetAllStorageAccounts()).ToList();
    }
}
