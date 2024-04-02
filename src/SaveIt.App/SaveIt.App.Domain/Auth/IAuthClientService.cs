namespace SaveIt.App.Domain.Auth;

public interface IAuthClientService
{
    Task<Uri> GetAuthorizationUrlAsync(Guid requestId);
}