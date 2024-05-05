using System.Security.Cryptography;

namespace SaveIt.Server.UI.Services;

public class OAuthStateProvider : IOAuthStateProvider
{
    /// <summary>
    /// Parameter with the length of the state parameter. Length can be from 43 to 128 characters.
    /// </summary>
    public const int StateParameterLength = 64;

    public string GetStateParameter()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[StateParameterLength];

        rng.GetBytes(bytes);

        var result =  BitConverter.ToString(bytes)
            .Replace("-", "")
            .ToLower();

        return result;
    }
}
