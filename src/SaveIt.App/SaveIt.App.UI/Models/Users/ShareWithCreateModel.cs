using FluentValidation;

namespace SaveIt.App.UI.Models.Users;
public class ShareWithCreateModel
{
    public string Email { get; set; } = default!;
}

public class ShareWithModelValidator : AbstractValidator<ShareWithCreateModel>
{
    public ShareWithModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Configure(x => x.MessageBuilder = _ => "Valid email address is required");
    }
}
