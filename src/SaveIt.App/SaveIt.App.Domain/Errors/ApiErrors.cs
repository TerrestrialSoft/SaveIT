using FluentResults;

namespace SaveIt.App.Domain.Errors;
public class ApiErrors(string message) : Error(message)
{
    public static GeneralError General() => new();
    public static IncompleteUploadError IncompleteUpload() => new();
    public static NotFoundError NotFound() => new();
    public static AuthorizationError InvalidAuthorization() => new();

    public class GeneralError(string? message = null) : ApiErrors("There was an error when working with external API. " + message)
    {
    }

    public class IncompleteUploadError(string? message = null) : ApiErrors("Incomplete upload error. " + message)
    {
    }

    public class NotFoundError(string? message = null) : ApiErrors("Not Found error. " + message)
    {
    }

    public class AuthorizationError(string? message = null) : ApiErrors("Authorization error. " + message)
    {
    }

}
