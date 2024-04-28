using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Entities;

namespace SaveIt.App.UI.Components.Modals;
public partial class AdvancedGameSaveSettingsModal
{
    public const string Title = "Advanced Game Save Settings";

    [Parameter, EditorRequired]
    public required Modal ModalCurrent { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalUploadGameSave { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalGameSaveVersions { get; set; }

    [Parameter, EditorRequired]
    public required Modal ModalDownloadGameSave { get; set; }

    [Parameter, EditorRequired]
    public required GameSave GameSave { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnGameSaveVersionsTemporarilyClosing { get; set; }

    private ConfirmDialog _confirmDialog = default!;

    private async Task ShowDownloadOlderVersionModal()
    {
        await ModalCurrent.HideAsync();
        var parameters = new Dictionary<string, object>
        {
            { nameof(GameSaveVersionsModal.GameSave), GameSave },
            { nameof(GameSaveVersionsModal.ModalCurrent), ModalGameSaveVersions },
            { nameof(GameSaveVersionsModal.ModalDownloadGameSave), ModalDownloadGameSave }
        };

        await ModalGameSaveVersions.ShowAsync<GameSaveVersionsModal>(GameSaveVersionsModal.Title, parameters: parameters);
    }

    private async Task UnlockRepositoryAsync()
    {

    }

    private async Task ShowManualUploadModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
        };

        await ModalGameSaveVersions.ShowAsync<GameSaveVersionsModal>(GameSaveVersionsModal.Title, parameters: parameters);
    }
}