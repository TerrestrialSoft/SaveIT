using SaveIt.Server.UI.Services;

namespace SaveIt.Server.Tests.Services;
public class OAuthStateProviderTests
{
    [Fact]
    public void GetSecurityToken_ShouldReturnCorrectLength()
    {
        // Arrange
        var provider = new OAuthStateProvider();

        // Act
        var token = provider.GetStateParameter();

        // Assert
        Assert.Equal(OAuthStateProvider.StateParameterLength * 2, token.Length);
    }

    [Fact]
    public void GetSecurityToken_ShouldReturnDifferentTokens()
    {
        // Arrange
        var provider = new OAuthStateProvider();

        // Act
        var token1 = provider.GetStateParameter();
        var token2 = provider.GetStateParameter();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GetSecurityToken_ShouldReturnDifferentTokensWithTheSameLength_WhenCalledMultipleTimes()
    {
        // Arrange
        var provider = new OAuthStateProvider();

        // Act
        var tokens = new List<string>();
        for (var i = 0; i < 100; i++)
        {
            tokens.Add(provider.GetStateParameter());
        }

        // Assert
        Assert.Equal(tokens.Count, tokens.Distinct().Count());
        Assert.Single(tokens.Select(x => x.Length).Distinct());
    }
}
