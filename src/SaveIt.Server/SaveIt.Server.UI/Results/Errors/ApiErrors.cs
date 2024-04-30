using FluentResults;

namespace SaveIt.Server.UI.Results.Errors;

public class ApiErrors(string message) : Error(message)
{
    public static GeneralError General() => new();
    public static UnAuthorizedError UnAuthorized() => new();
    public static ForbiddenError Forbidden() => new();
    public static NotFoundError NotFound() => new();
    public static InternalServerError InternalServer() => new();

    public class GeneralError(string? message = null) : ApiErrors("There was an error when working with the API. " + message)
    {
    }

    public class UnAuthorizedError(string? message = null) : ApiErrors("Unauthorized action error. " + message)
    {
    }

    public class ForbiddenError(string? message = null) : ApiErrors("Forbidden action error. " + message)
    {
    }

    public class NotFoundError(string? message = null) : ApiErrors("Not Found error. " + message)
    {
    }

    public class InternalServerError(string? message = null) : ApiErrors("Internal Server error. " + message)
    {
    }

}
