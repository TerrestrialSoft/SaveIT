using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace SaveIt.App.UI.Extensions;
public static class NavigationManagerExtensions
{
    public static bool TryGetQueryParameter<T>(this NavigationManager navManager, string key, out T? value)
    {
        value = default;
        var uri = navManager.ToAbsoluteUri(navManager.Uri);

        if (!QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var val))
        {
            return false;
        }
        
        if (typeof(T) == typeof(string))
        {
            value = (T)(object)val.ToString();
            return true;
        }

        if (typeof(T) == typeof(Guid) && Guid.TryParse(val, out var result))
        {
            value = (T)(object)result;
            return true;
        }

        return false;
    }
}
