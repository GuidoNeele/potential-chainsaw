namespace RazorBlade;

/// <summary>
/// Base class for HTML Razor templates.
/// </summary>
public abstract class HtmlTemplate : RazorTemplate, IEncodedContent
{
    /// <summary>
    /// Writes this template to a TextWriter.
    /// </summary>
    public void WriteTo(TextWriter textWriter)
    {
        Output = textWriter;
        ExecuteAsync().GetAwaiter().GetResult();
    }
}

/// <summary>
/// Base class for HTML Razor templates with a model.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class HtmlTemplate<TModel> : HtmlTemplate
{
    /// <summary>
    /// The model for this template.
    /// </summary>
    public TModel Model { get; }

    /// <summary>
    /// Creates a new instance with the specified model.
    /// </summary>
    protected HtmlTemplate(TModel model)
    {
        Model = model;
    }
}
