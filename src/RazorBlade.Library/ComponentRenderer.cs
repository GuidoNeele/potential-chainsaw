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

    /// <summary>
    /// Renders a component with an optional layout.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <typeparam name="TLayout">The type of layout component (must have a Body RenderFragment parameter).</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <param name="layoutParameters">The parameters to pass to the layout.</param>
    /// <returns>The rendered HTML as a string.</returns>
    public static async Task<string> RenderComponentWithLayoutAsync<TComponent, TLayout>(
        Dictionary<string, object?>? parameters = null,
        Dictionary<string, object?>? layoutParameters = null) 
        where TComponent : IComponent
        where TLayout : IComponent
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.AddConsole());
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);
        
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            // First render the content component
            var contentParams = parameters != null 
                ? ParameterView.FromDictionary(parameters) 
                : ParameterView.Empty;
            var contentOutput = await htmlRenderer.RenderComponentAsync<TComponent>(contentParams);
            var contentHtml = contentOutput.ToHtmlString();

            // Create a RenderFragment for the content
            RenderFragment bodyFragment = builder =>
            {
                builder.AddMarkupContent(0, contentHtml);
            };

            // Prepare layout parameters with the Body
            var layoutDict = layoutParameters != null 
                ? new Dictionary<string, object?>(layoutParameters) 
                : new Dictionary<string, object?>();
            layoutDict["Body"] = bodyFragment;

            // Render the layout with the content
            var layoutParamView = ParameterView.FromDictionary(layoutDict);
            var layoutOutput = await htmlRenderer.RenderComponentAsync<TLayout>(layoutParamView);
            return layoutOutput.ToHtmlString();
        });

        return html;
    }

    /// <summary>
    /// Renders a component from a view model, mapping properties to component parameters.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to render.</typeparam>
    /// <typeparam name="TViewModel">The type of view model.</typeparam>
    /// <param name="viewModel">The view model instance.</param>
    /// <returns>The rendered HTML as a string.</returns>
    public static async Task<string> RenderComponentFromViewModelAsync<TComponent, TViewModel>(TViewModel viewModel) 
        where TComponent : IComponent
        where TViewModel : class
    {
        // Map view model properties to component parameters
        var parameters = new Dictionary<string, object?>();
        var viewModelType = typeof(TViewModel);
        var properties = viewModelType.GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(viewModel);
            parameters[prop.Name] = value;
        }

        return await RenderComponentAsync<TComponent>(parameters);
    }
}
