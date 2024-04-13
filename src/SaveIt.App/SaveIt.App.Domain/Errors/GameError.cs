using FluentResults;

namespace SaveIt.App.Domain.Errors;
public class GameError(string message) : Error(message)
{
    public static GameSaveInUseError GameSaveInUse => new();

    public class GameSaveInUseError : GameError
    {
        public GameSaveInUseError() : base("Game save is already in use")
        {
        }
    }
}
