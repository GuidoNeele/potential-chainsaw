using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RazorBlade;

/// <summary>
/// Utility for rendering Razor components to HTML strings.
/// </summary>
public static class ComponentRenderer
{
    /// <summary>
    /// Renders a Razor component to an HTML string.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <param name="component">The component instance to render.</param>
    /// <returns>The rendered HTML as a string.</returns>
    public static async Task<string> RenderComponentAsync<TComponent>(TComponent component) 
        where TComponent : IComponent
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.AddConsole());
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);
        
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.Empty;
            var output = await htmlRenderer.RenderComponentAsync(component.GetType(), parameters);
            return output.ToHtmlString();
        });

        return html;
    }

    /// <summary>
    /// Renders a Razor component to an HTML string with parameters.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>The rendered HTML as a string.</returns>
    public static async Task<string> RenderComponentAsync<TComponent>(Dictionary<string, object?>? parameters = null) 
        where TComponent : IComponent
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.AddConsole());
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);
        
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameterView = parameters != null 
                ? ParameterView.FromDictionary(parameters) 
                : ParameterView.Empty;
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameterView);
            return output.ToHtmlString();
        });

        return html;
    }
}
