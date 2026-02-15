using System.Text;

namespace RazorBlade;

/// <summary>
/// Interface for content that does not need to be HTML-escaped.
/// </summary>
public interface IEncodedContent
{
    /// <summary>
    /// Writes the content to the provided TextWriter.
    /// </summary>
    void WriteTo(TextWriter textWriter);
}
