namespace SaveIt.Server.UI.Services;

/// <summary>
/// Represents a provider for OAuth state parameter.
/// </summary>
public interface IOAuthStateProvider
{
    /// <summary>
    /// Generates and returns state parameter.
    /// </summary>
    /// <returns>State parameter</returns>
    string GetStateParameter();
}