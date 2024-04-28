using FluentValidation;

namespace SaveIt.App.UI.Models.GameSaves;
public class GameSaveVersionsCountModel
{
    public int Count { get; set; }
}

public class GameSaveVersionsCountModelValidator : AbstractValidator<GameSaveVersionsCountModel>
{
    public GameSaveVersionsCountModelValidator()
    {
        RuleFor(x => x.Count)
            .Must(x => x >= 1).WithMessage("The count must be greater than or equal to 1.")
            .Must(x => x <= 100).WithMessage("The count must be less than or equal to 100.");
    }
}
