using BlazorBootstrap;
using FluentResults;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Attributes;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;

namespace SaveIt.App.UI.Components.Modals.GameSaves;
public partial class StartGameSaveModal
{
    public const string Title = "Play Game";

    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required Guid SaveId { get; set; }

    [Parameter, EditorRequired]
    public required string DefaultGameName { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnClose { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<GameSave> OnGameSaveUpdate { get; set; }


    private GameSave _gameSave = default!;
    private StartGameScreenState _screenState = StartGameScreenState.Loading;
    private LockFileModel? _lockFile;
    private string? _errorMessage;
    private bool _loading = false;

    protected override async Task OnInitializedAsync()
    {
        _loading = false;
        _errorMessage = null;
        _lockFile = null;
        _screenState = StartGameScreenState.Loading;
        _gameSave = (await GameSaveRepository.GetWithChildrenAsync(SaveId))!;

        await LockRepositoryAsync();
    }

    private async Task LockRepositoryAsync()
    {
        var result = await GameService.LockRepositoryAsync(SaveId);

        if (result.IsSuccess)
        {
            await PrepareSaveAsync();
            return;
        }

        if (result.HasError<GameErrors.GameSaveInUseError>(out var errors))
        {
            var error = errors.First();
            _lockFile = error.LockFile;
            _screenState = StartGameScreenState.SaveInUse;

            return;
        }

        if (result.HasError<GameErrors.GameSaveAlreadyLocked>())
        {
            await StartGameAndContinueWithAsync(StartGameScreenState.HostingGame);
            return;
        }

        _screenState = StartGameScreenState.LockFailed;
        _errorMessage = result.Errors[0].Message;
    }

    private async Task PrepareSaveAsync()
    {
        _errorMessage = null;
        _screenState = StartGameScreenState.DownloadingSave;
        StateHasChanged();
        var result = await GameService.PrepareGameSaveAsync(SaveId);

        if (result.IsFailed)
        {
            _screenState = StartGameScreenState.DownloadFailed;
            _errorMessage = result.Errors[0].Message;
            return;
        }

        _screenState = StartGameScreenState.HostingGame;
        await OnGameSaveUpdate.InvokeAsync(_gameSave);
        await StartGameAndContinueWithAsync(StartGameScreenState.HostingGame);
    }

    private async Task StartGameAndContinueWithAsync(StartGameScreenState state)
    {
        _errorMessage = null;
        var result = await StartGameAsync();
        if (result.IsFailed)
        {
            _errorMessage = "In case the game failed to start automatically, please start it manually.";
        }

        _screenState = state;
    }

    private async Task<Result> StartGameAsync()
    {
        if (_gameSave.Game.GameExecutablePath is null)
        {
            return Result.Ok();
        }

        var result = await GameService.StartGameAsync(_gameSave.Game.Id);

        if (result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to start the game."));
        }

        return result;
    }

    private Task CloseAsync()
        => OnClose.InvokeAsync();

    private async Task DiscardProgressAndCloseAsync()
    {
        _loading = true;

        var result = await GameService.UnlockRepositoryAsync(SaveId);
        if (result.IsFailed)
        {
            string? errorMessage = null;
            if (result.HasError<GameErrors.GameLockedByDifferentUser>(out var error))
            {
                errorMessage = error.First().Message;
            }

            errorMessage ??= "Failed to unlock the repository.";

            ToastService.Notify(new ToastMessage(ToastType.Danger, errorMessage));
        }

        _loading = false;
        await CloseAsync();
    }

    private async Task UploadSaveAndCloseAsync()
    {
        _loading = true;
        var result = await GameService.UploadGameSaveAsync(SaveId);

        if (result.IsSuccess)
        {
            _loading = false;
            ToastService.Notify(new ToastMessage(ToastType.Success, "Game Save successfully uploaded"));
            await CloseAsync();
            return;
        }

        _loading = false;
        _errorMessage = result.Errors[0].Message;
        _screenState = StartGameScreenState.UploadFailed;
    }

    private static string GetStateColorClass(StartGameScreenState state)
        => state switch
        {
            StartGameScreenState.SaveInUse => "text-warning",
            var v when v == StartGameScreenState.LockFailed || v == StartGameScreenState.UploadFailed
                || v == StartGameScreenState.DownloadFailed => "text-danger",
            var v when v == StartGameScreenState.PlayingGame || v == StartGameScreenState.HostingGame => "text-success",
            StartGameScreenState.HostingGame => "text-success",
            _ => ""
        };

    private enum StartGameScreenState
    {
        [Name("Locking the Cloud Storage...")]
        Loading = 1,

        [Name("Downloading the Game Save...")]
        DownloadingSave = 2,

        [Name("Game Save in Use")]
        SaveInUse = 3,

        [Name("Starting the Game...")]
        StartingGame = 4,

        [Name("Playing the Game")]
        PlayingGame = 5,

        [Name("Hosting the Game")]
        HostingGame = 6,

        [Name("Error")]
        LockFailed = 7,

        [Name("Upload Failed")]
        UploadFailed = 8,

        [Name("Download Failed")]
        DownloadFailed = 9
    }
}