using FluentValidation;

namespace SaveIt.App.UI.Models.Games;
public class DownloadGameSaveModel
{
    public bool SetAsActiveGameSave { get; set; }
    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;
}

public class DownloadGameSaveModelValidator : AbstractValidator<DownloadGameSaveModel>
{
    public DownloadGameSaveModelValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.SetAsActiveGameSave && x.LocalGameSaveFile is not null
                || x.SetAsActiveGameSave && x.LocalGameSaveFile is null)
            .WithMessage("Local Game Save must be set.");
    }
}
