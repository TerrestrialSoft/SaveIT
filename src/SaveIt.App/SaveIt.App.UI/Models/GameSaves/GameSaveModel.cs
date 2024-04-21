using FluentValidation;

namespace SaveIt.App.UI.Models.GameSaves;
public class GameSaveModel
{
    public string Name { get; set; } = default!;

    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;

    public RemoteFileItemModel? RemoteGameSaveFile { get; set; } = default!;

    public Guid? StorageAccountId { get; set; }
}

public class CreateGameSaveModelValidator : AbstractValidator<GameSaveModel>
{
    public CreateGameSaveModelValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty().WithMessage("You have to enter the Game Save Name.")
            .MaximumLength(50).WithMessage("Game Save Name cannot be longer than 50 characters");
        RuleFor(x => x.LocalGameSaveFile)
            .NotNull().WithMessage("Local Game Save File is required.");
        RuleFor(x => x.RemoteGameSaveFile)
            .NotNull().WithMessage("Remote Game Save File is required.");
        RuleFor(x => x.StorageAccountId)
            .NotNull().WithMessage("Storage Account is required.");
    }
}
