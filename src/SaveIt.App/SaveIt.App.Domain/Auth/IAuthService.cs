using FluentResults;

namespace SaveIt.App.Domain.Auth;

public interface IAuthService
{
    Task<Result<Uri>> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken);
    
    Task<Result> WaitForAuthorizationAsync(Guid requestId, CancellationToken cancellationToken);
}
