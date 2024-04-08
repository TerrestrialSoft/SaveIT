using FluentResults;

namespace SaveIt.App.Domain.Errors;
public class AuthError : Error
{
    public AuthError(string message) : base(message)
    {
    }
}
