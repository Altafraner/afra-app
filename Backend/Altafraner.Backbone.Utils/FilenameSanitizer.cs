namespace Altafraner.Backbone.Utils;

/// <summary>
///     A quick utility class to sanitize filenames
/// </summary>
public static class FilenameSanitizer
{
    private static readonly char[] AllowedChars = ['_', '-', '.', ' '];

    /// <summary>
    ///     Sanitizes a given filename
    /// </summary>
    public static string Sanitize(string input)
    {
        return new string(input.Select(c => char.IsLetterOrDigit(c) || AllowedChars.Contains(c) ? c : '_')
            .ToArray()).Trim(AllowedChars);
    }
}
