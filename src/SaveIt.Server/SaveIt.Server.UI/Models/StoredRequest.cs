namespace SaveIt.Server.UI.Models;

public record StoredRequest(StateModel State, OAuthTokenModel? Token = null);
