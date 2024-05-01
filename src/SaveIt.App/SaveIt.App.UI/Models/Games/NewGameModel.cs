using FluentValidation;
using SaveIt.App.UI.Models.GameSaves;

namespace SaveIt.App.UI.Models.Games;
public class NewGameModel
{
    public GameModel Game { get; set; } = new();
    public GameSaveModel GameSave { get; set; } = new();
}

public class NewGameModelValidator : AbstractValidator<NewGameModel>
{
    public NewGameModelValidator()
    {
        RuleFor(x => x.Game).SetValidator(new CreateGameModelValidator());
        RuleFor(x => x.GameSave).SetValidator(new CreateGameSaveModelValidator());
    }
}