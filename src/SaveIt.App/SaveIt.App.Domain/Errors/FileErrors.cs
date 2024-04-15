using FluentResults;

namespace SaveIt.App.Domain.Errors;
public abstract class FileErrors(string message) : Error(message)
{
    public static GeneralError General() => new();
    public static LocationNotFoundError LocationNotFound() => new();

    public class GeneralError(string? message = null) : FileErrors("There was an error when working with the file. " + message)
    {
    }

    public class LocationNotFoundError(string? message = null) : FileErrors("Location not found. " + message)
    {
    }
}