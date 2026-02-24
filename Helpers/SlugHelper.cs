using System.Text.RegularExpressions;

namespace SmartWinners.Helpers;

public static class SlugHelper
{
    private static readonly Regex InvalidCharacters = new("[^a-z0-9\\s-]", RegexOptions.Compiled);
    private static readonly Regex Whitespace = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex DuplicateHyphen = new("-{2,}", RegexOptions.Compiled);

    public static string ToSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var working = value.Trim().ToLowerInvariant()
            .Replace("&", " and ")
            .Replace('+', ' ')
            .Replace('/', ' ')
            .Replace('\\', ' ')
            .Replace('.', ' ')
            .Replace('_', ' ');

        working = InvalidCharacters.Replace(working, " ");
        working = Whitespace.Replace(working, " ").Trim();

        if (string.IsNullOrWhiteSpace(working))
            return string.Empty;

        var slug = working.Replace(' ', '-');
        slug = DuplicateHyphen.Replace(slug, "-");
        return slug.Trim('-');
    }
}
