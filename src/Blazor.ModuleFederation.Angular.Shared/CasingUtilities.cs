using System.Text.RegularExpressions;

namespace Blazor.ModuleFederation.Angular.Shared;

public static class CasingUtilities
{
    public static string PascalToCamelCase(string s)
    {
        return char.ToLowerInvariant(s[0]) + s[1..];
    }

    public static string ToKebabCase(string s)
    {
        return Regex.Replace(s, "[A-Z]+(?![a-z])|[A-Z]", EvaluateKebabCaseMatch);
    }

    private static string EvaluateKebabCaseMatch(Match match)
    {
        return (match.Index > 0 ? "-" : string.Empty) + match.Value.ToLowerInvariant();
    }
}
