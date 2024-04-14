using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Errors;
public class GameError(string message) : Error(message)
{
    public static GameSaveInUseError GameSaveInUse (LockFileModel lockFile) => new(lockFile);

    public class GameSaveInUseError(LockFileModel lockFile) : GameError("Game save is already in use.")
    {
        public LockFileModel LockFile { get; } = lockFile;
    }
}
