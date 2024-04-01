namespace SaveIt.App.Domain.Auth;

public interface IAuthService
{
    Task<Uri> GetAuthorizationUrlAsync();
}
