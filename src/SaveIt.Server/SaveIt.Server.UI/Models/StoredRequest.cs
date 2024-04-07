namespace SaveIt.Server.UI.Models;

public record StoredRequest(StateModel State, OAuthCompleteTokenResponseModel? Token = null);
