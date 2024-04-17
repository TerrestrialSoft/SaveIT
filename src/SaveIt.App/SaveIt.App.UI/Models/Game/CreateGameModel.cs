using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace SaveIt.App.UI.Models.Game;
public class CreateGameModel
{
    public string Name { get; set; } = default!;

    public string Username { get; set; } = default!;

    public LocalFileItemModel? GameExecutableFile { get; set; }

    public ImageModel? Image { get; set; }
}

public class CreateGameModelValidator : AbstractValidator<CreateGameModel>
{
    public CreateGameModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("You have to enter the Game name.")
            .MaximumLength(50).WithMessage("Game name cannot be longer than 50 characters");

        RuleFor(p => p.Username)
            .NotEmpty().WithMessage("You have to enter your Username.")
            .MaximumLength(30).WithMessage("Username cannot be longer than 30 characters");
    }
}
