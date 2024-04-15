using FluentResults;

namespace SaveIt.App.Domain.Errors;
public class AuthError(string message) : Error(message)
{
}
