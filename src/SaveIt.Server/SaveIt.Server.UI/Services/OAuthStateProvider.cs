using System.Security.Cryptography;

namespace SaveIt.Server.UI.Services;

public class OAuthStateProvider : IOAuthStateProvider
{
    // Length can be in [43; 128] characters
    private const int _stateParameterLength = 128;

    public string GetSecurityToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[_stateParameterLength];

        rng.GetBytes(bytes);

        return BitConverter.ToString(bytes)
            .Replace("-", "")
            .ToLower();
    }
}
