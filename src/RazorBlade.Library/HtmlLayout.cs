using System.Text;

namespace RazorBlade;

/// <summary>
/// Base class for HTML layout templates.
/// </summary>
public abstract class HtmlLayout : HtmlTemplate
{
    private HtmlTemplate? _body;
    private readonly Dictionary<string, HtmlTemplate?> _sections = new();

    /// <summary>
    /// Gets or sets the body content.
    /// </summary>
    public HtmlTemplate? Body
    {
        get => _body;
        set => _body = value;
    }

    /// <summary>
    /// Renders the body content.
    /// </summary>
    protected void RenderBody()
    {
        if (_body != null)
            _body.WriteTo(Output);
    }

    /// <summary>
    /// Defines a section.
    /// </summary>
    protected void DefineSection(string name, HtmlTemplate? content)
    {
        _sections[name] = content;
    }

    /// <summary>
    /// Renders a section.
    /// </summary>
    protected void RenderSection(string name, bool required = true)
    {
        if (_sections.TryGetValue(name, out var section) && section != null)
        {
            section.WriteTo(Output);
        }
        else if (required)
        {
            throw new InvalidOperationException($"Section '{name}' is not defined.");
        }
    }

    /// <summary>
    /// Checks if a section is defined.
    /// </summary>
    protected bool IsSectionDefined(string name)
    {
        return _sections.ContainsKey(name) && _sections[name] != null;
    }
}

/// <summary>
/// Base class for HTML templates with an automatic layout.
/// </summary>
/// <typeparam name="TLayout">The layout type.</typeparam>
public abstract class HtmlTemplateWithLayout<TLayout> : HtmlTemplate
    where TLayout : HtmlLayout, new()
{
    /// <summary>
    /// Creates the layout for this template.
    /// </summary>
    protected virtual HtmlLayout CreateLayout()
    {
        return new TLayout();
    }

    /// <summary>
    /// Renders the template with its layout.
    /// </summary>
    public new string Render()
    {
        var layout = CreateLayout();
        layout.Body = this;
        return layout.Render();
    }

    /// <summary>
    /// Renders the template with its layout asynchronously.
    /// </summary>
    public new async Task<string> RenderAsync()
    {
        var layout = CreateLayout();
        layout.Body = this;
        return await layout.RenderAsync();
    }
}
