using System.Net;
using System.Text;

namespace RazorBlade;

/// <summary>
/// Helper class for HTML encoding.
/// </summary>
public static class Html
{
    /// <summary>
    /// Returns an HtmlString that will not be HTML-escaped.
    /// </summary>
    public static HtmlString Raw(string? value)
        => new(value ?? string.Empty);

    /// <summary>
    /// HTML-encodes a string.
    /// </summary>
    public static string Encode(string? value)
        => value == null ? string.Empty : WebUtility.HtmlEncode(value);
}
