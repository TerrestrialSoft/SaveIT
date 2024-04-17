using FluentValidation;
using SaveIt.App.UI.Models.GameSave;

namespace SaveIt.App.UI.Models.Game;
public class NewGameModel
{
    public CreateGameModel Game { get; set; } = new();
    public CreateGameSaveModel GameSave { get; set; } = new();
}

public class NewGameModelValidator : AbstractValidator<NewGameModel>
{
    public NewGameModelValidator()
    {
        RuleFor(x => x.Game).SetValidator(new CreateGameModelValidator());
        RuleFor(x => x.GameSave).SetValidator(new CreateGameSaveModelValidator());
    }
}