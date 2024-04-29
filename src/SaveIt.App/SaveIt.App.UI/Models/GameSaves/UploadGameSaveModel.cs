using FluentValidation;

namespace SaveIt.App.UI.Models.GameSaves;
public class UploadGameSaveModel
{
    public LocalFileItemModel? File { get; set; }
}

public class UploadGameSaveModelValidator : AbstractValidator<UploadGameSaveModel>
{
    public UploadGameSaveModelValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Please select a file to upload.");
    }
}
