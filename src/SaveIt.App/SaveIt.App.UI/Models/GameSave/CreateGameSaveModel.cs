using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace SaveIt.App.UI.Models.GameSave;
public class CreateGameSaveModel
{
    [Required(ErrorMessage = "Game Save Name is required.")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Local Game Save File is required.")]
    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;

    [Required(ErrorMessage = "Remote Game Save File is required.")]
    public RemoteFileItemModel? RemoteGameSaveFile { get; set; } = default!;

    [Required(ErrorMessage = "Storage Account is required.")]
    public Guid? StorageAccountId { get; set; }
}

public class CreateGameSaveModelValidator : AbstractValidator<CreateGameSaveModel>
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
