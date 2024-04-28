using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.UI.Components.Modals;
public partial class DownloadGameSaveModal
{
    public const string Title = "Download Game Save";

    [Parameter, EditorRequired]
    public required Guid StorageAccountId { get; set; }

    [Parameter, EditorRequired]
    public required FileItemModel FileItem { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnDownload { get; set; }

    [Parameter, EditorRequired]
    public required Modal CurrentModal { get; set; }

    private bool _lockRepository = false;

    private Task HideModalAsync()
        => CurrentModal.HideAsync();

    private async Task DownloadSaveAsync()
    {

    }
}