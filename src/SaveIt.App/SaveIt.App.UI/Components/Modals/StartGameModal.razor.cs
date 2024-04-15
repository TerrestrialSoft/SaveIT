using BlazorBootstrap;
using FluentResults;
using Microsoft.AspNetCore.Components;
using SaveIt.App.Domain.Attributes;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;

namespace SaveIt.App.UI.Components.Modals;
public partial class StartGameModal
{
    public const string Title = "Play Game";

    [Parameter, EditorRequired]
    public required Guid SaveId { get; set; }

    [Parameter, EditorRequired]
    public required string DefaultGameName { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnClose { get; set; }

    [Inject]
    public IGameSaveRepository GameSaveRepository { get; set; } = default!;

    [Inject]
    public IGameService GameService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    private GameSave _gameSave = default!;
    private StartGameScreenState _screenState = StartGameScreenState.Loading;
    private LockFileModel? _lockFile;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        _errorMessage = null;
        _lockFile = null;
        _screenState = StartGameScreenState.Loading;
        _gameSave = (await GameSaveRepository.GetGameSaveWithChildrenAsync(SaveId))!;

        await LockRepositoryAsync();
    }

    private async Task LockRepositoryAsync()
    {
        var result = await GameService.LockRepositoryAsync(SaveId);

        if (result.IsFailed)
        {
            if (result.HasError<GameErrors.GameSaveInUseError>(out var errors))
            {
                var error = errors.First();
                _lockFile = error.LockFile;
                _screenState = StartGameScreenState.SaveInUse;

                return;
            }

            if(result.HasError<GameErrors.GameSaveAlreadyLocked>())
            {
                await StartGameAndContinueWithAsync(StartGameScreenState.HostingGame);
                return;
            }

            _screenState = StartGameScreenState.Error;
            _errorMessage = result.Errors[0].Message;
        }
    }

    private async Task StartGameAndContinueWithAsync(StartGameScreenState state)
    {
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

        if(result.IsFailed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to start the game."));
        }

        return result;
    }

    private async Task CloseAsync()
    {
        await OnClose.InvokeAsync();
    }

    private async Task DiscardProgress()
    {
        var result = await GameService.UnlockRepositoryAsync(SaveId);
        if (result.IsFailed)
        {
            string? errorMessage = null;
            if(result.HasError<GameErrors.GameLockedByDifferentUser>(out var error))
            {
                errorMessage = error.First().Message;
            }

            errorMessage ??= "Failed to unlock the repository.";

            ToastService.Notify(new ToastMessage(ToastType.Danger, errorMessage));
        }

        await CloseAsync();
    }

    private async Task UploadSaveAsync()
    {
        var result = await GameService.UploadSaveAsync(SaveId);
    }

    private static string GetStateColorClass(StartGameScreenState state)
        => state switch
        {
            StartGameScreenState.SaveInUse => "text-warning",
            StartGameScreenState.Error => "text-danger",
            var v when (v == StartGameScreenState.PlayingGame || v == StartGameScreenState.HostingGame) => "text-success",
            StartGameScreenState.HostingGame => "text-success",
            _ => ""
        };

    private enum StartGameScreenState
    {
        [Name("Loading...")]
        Loading = 1,

        [Name("Downloading Save...")]
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
        Error = 7,
    }
}