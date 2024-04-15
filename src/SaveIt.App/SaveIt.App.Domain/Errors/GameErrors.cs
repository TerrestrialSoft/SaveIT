using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Errors;
public abstract class GameErrors(string message) : Error(message)
{
    public static GameSaveInUseError GameSaveInUse (LockFileModel lockFile) => new(lockFile);
    public static GameSaveInUseError CurrentUserLockedGameSave (LockFileModel lockFile) => new(lockFile);
    public static GameSaveInUseError GameLockedByAnotherUser (LockFileModel lockFile) => new(lockFile);

    public class GameSaveInUseError(LockFileModel lockFile) : GameErrors("Game save is already in use.")
    {
        public LockFileModel LockFile { get; } = lockFile;
    }

    public class GameSaveAlreadyLocked(LockFileModel lockFile) : GameErrors("You have already locked the game save.")
    {
        public LockFileModel LockFile { get; } = lockFile;
    }

    public class GameLockedByDifferentUser(LockFileModel lockFile) : GameErrors("Game save is locked by a different user.")
    {
        public LockFileModel LockFile { get; } = lockFile;
    }
}
