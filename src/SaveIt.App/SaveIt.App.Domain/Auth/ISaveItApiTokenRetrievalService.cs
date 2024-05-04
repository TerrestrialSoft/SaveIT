using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface ISaveItApiTokenRetrievalService
{
    Task<OAuthCompleteTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken = default);
}
