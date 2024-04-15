using System.Reflection;

namespace SaveIt.App.Domain.Attributes;
public static class AttributeExtensions
{
    public static string GetName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString())!;
        var attribute = field.GetCustomAttribute<NameAttribute>();

        return attribute?.Name ?? value.ToString();
    }
}
