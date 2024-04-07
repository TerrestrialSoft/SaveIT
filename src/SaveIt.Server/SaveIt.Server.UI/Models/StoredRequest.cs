namespace SaveIt.Server.UI.Models;

public record StoredRequest(StateModel State, OAuthAccessTokenResponseModel? Token = null);
