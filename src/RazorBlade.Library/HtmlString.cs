using System.Text;

namespace RazorBlade;

/// <summary>
/// Represents a string that should not be HTML-escaped.
/// </summary>
public sealed class HtmlString : IEncodedContent
{
    private readonly string _value;

    public HtmlString(string value)
    {
        _value = value ?? string.Empty;
    }

    public void WriteTo(TextWriter textWriter)
    {
        textWriter.Write(_value);
    }

    public override string ToString() => _value;
}
