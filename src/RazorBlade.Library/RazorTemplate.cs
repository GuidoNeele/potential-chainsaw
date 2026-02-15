using System.Text;

namespace RazorBlade;

/// <summary>
/// Base class for Razor templates.
/// </summary>
public abstract class RazorTemplate
{
    private TextWriter? _output;

    /// <summary>
    /// The TextWriter to write output to.
    /// </summary>
    protected TextWriter Output
    {
        get => _output ?? throw new InvalidOperationException("Output not initialized");
        set => _output = value;
    }

    /// <summary>
    /// Executes the template and writes the output.
    /// </summary>
    protected internal abstract Task ExecuteAsync();

    /// <summary>
    /// Renders the template to a string.
    /// </summary>
    public string Render()
    {
        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
        {
            _output = writer;
            ExecuteAsync().GetAwaiter().GetResult();
        }
        return builder.ToString();
    }

    /// <summary>
    /// Renders the template to a string asynchronously.
    /// </summary>
    public async Task<string> RenderAsync()
    {
        var builder = new StringBuilder();
        await using (var writer = new StringWriter(builder))
        {
            _output = writer;
            await ExecuteAsync();
        }
        return builder.ToString();
    }

    /// <summary>
    /// Writes a literal string to the output.
    /// </summary>
    protected void WriteLiteral(string? value)
    {
        if (value != null)
            Output.Write(value);
    }

    /// <summary>
    /// Writes a value to the output, HTML-encoding it if necessary.
    /// </summary>
    protected void Write(object? value)
    {
        if (value == null)
            return;

        if (value is IEncodedContent encodedContent)
        {
            encodedContent.WriteTo(Output);
        }
        else
        {
            Output.Write(Html.Encode(value.ToString()));
        }
    }

    /// <summary>
    /// Writes a string to the output, HTML-encoding it.
    /// </summary>
    protected void Write(string? value)
    {
        if (value != null)
            Output.Write(Html.Encode(value));
    }

    /// <summary>
    /// Begins writing an attribute.
    /// </summary>
    protected void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
    {
        Output.Write(prefix);
    }

    /// <summary>
    /// Writes an attribute value.
    /// </summary>
    protected void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
    {
        Output.Write(prefix);
        if (value != null)
        {
            if (isLiteral)
                Output.Write(value);
            else
                Write(value);
        }
    }

    /// <summary>
    /// Ends writing an attribute.
    /// </summary>
    protected void EndWriteAttribute()
    {
        // Nothing to do
    }
}
