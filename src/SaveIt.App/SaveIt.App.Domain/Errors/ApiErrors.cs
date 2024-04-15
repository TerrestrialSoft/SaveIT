using FluentResults;

namespace SaveIt.App.Domain.Errors;
public class ApiErrors(string message) : Error(message)
{
    public static GeneralError General() => new();
    public static IncompleteUploadError IncompleteUpload() => new();
    public static NotFoundError NotFound() => new();

    public class GeneralError(string? message = null) : FileErrors("There was an error when working with the file. " + message)
    {
    }

    public class IncompleteUploadError(string? message = null) : FileErrors("Incomplete upload error. " + message)
    {
    }

    public class NotFoundError(string? message = null) : FileErrors("Not Found error. " + message)
    {
    }

}
