using FluentValidation;

namespace SaveIt.App.UI.Models.GameSaves;
public class NewGameSaveModel
{
    public Guid? GameId { get; set; }
    public GameSaveModel GameSave { get; set; } = new();
}

public class NewGameSaveModelValidator : AbstractValidator<NewGameSaveModel>
{
    public NewGameSaveModelValidator()
    {
        RuleFor(x => x.GameId)
            .NotNull();
        RuleFor(x => x.GameSave).SetValidator(new CreateGameSaveModelValidator());
    }
}
