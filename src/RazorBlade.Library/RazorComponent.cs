using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorBlade;

/// <summary>
/// Base class for Razor components that can be rendered to HTML strings.
/// </summary>
public abstract class RazorComponent : ComponentBase
{
    /// <summary>
    /// Renders the component to an HTML string.
    /// </summary>
    /// <returns>The rendered HTML as a string.</returns>
    public async Task<string> RenderAsync()
    {
        return await ComponentRenderer.RenderComponentAsync(this);
    }

    /// <summary>
    /// Renders the component to an HTML string synchronously.
    /// </summary>
    /// <returns>The rendered HTML as a string.</returns>
    public string Render()
    {
        return RenderAsync().GetAwaiter().GetResult();
    }
}
