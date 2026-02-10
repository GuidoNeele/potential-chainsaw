# RazorBlade Sample Application

This sample demonstrates how to use RazorBlade to render Razor components in minimal API endpoints.

## What's Included

### Components

1. **Greeting.razor** - Simple greeting component with a Name parameter
2. **Card.razor** - Card component with Title, Content, and Footer parameters
3. **ItemList.razor** - List component that displays a collection of items
4. **DemoPage.razor** - Demo page component that includes other components
5. **Layout.razor** - Layout component for wrapping pages with HTML structure

### Endpoints

All endpoints are defined in `Program.cs`:

- `GET /` - Returns a greeting component
- `GET /card/{title}` - Returns a card with dynamic title and timestamp
- `GET /items` - Returns a shopping list
- `GET /demo` - Returns the demo page with full layout
- `GET /demo/partial` - Returns the demo page without layout (for htmx)
- `GET /card-vm/{title}` - Returns a card rendered from a view model

## How It Works

1. **Write Razor Components**: Components are defined in the `Components/` directory as `.razor` files
2. **Source Generation**: The RazorBlade source generator automatically creates extension methods during compilation
3. **Use in APIs**: Import `RazorBlade.Extensions` and call the generated `Render*Async()` methods

## Example Flow

### 1. Define Component (`Components/Greeting.razor`)

```razor
<div class="greeting">
    <h1>Hello, @Name!</h1>
</div>

@code {
    [Parameter]
    public string Name { get; set; } = "World";
}
```

### 2. Generated Code (Automatic)

The source generator creates:

```csharp
namespace RazorBlade.Extensions
{
    public static class GreetingExtensions
    {
        public static async Task<string> RenderGreetingAsync(string name) { ... }
        public static string RenderGreeting(string name) { ... }
    }
}
```

### 3. Use in Minimal API (`Program.cs`)

```csharp
using RazorBlade.Extensions;

app.MapGet("/greeting/{name}", async (string name) =>
{
    var html = await GreetingExtensions.RenderGreetingAsync(name);
    return Results.Content(html, "text/html");
});
```

## Running the Sample

```bash
dotnet run
```

Then navigate to:
- http://localhost:5018/ - Greeting component
- http://localhost:5018/demo - Demo page with full layout
- http://localhost:5018/demo/partial - Demo page as partial (no layout)
- http://localhost:5018/card-vm/Test - Card from view model

## Layout Support

The demo page can be rendered two ways:

### Full Page with Layout
```csharp
app.MapGet("/demo", async () =>
{
    var layoutParams = new Dictionary<string, object?> { { "Title", "RazorBlade Demo" } };
    var html = await DemoPageExtensions.RenderDemoPageWithLayoutAsync<Layout>(layoutParams);
    return Results.Content(html, "text/html");
});
```

### Partial (No Layout) for htmx
```csharp
app.MapGet("/demo/partial", async () =>
{
    var html = await DemoPageExtensions.RenderDemoPageAsync();
    return Results.Content(html, "text/html");
});
```

## View Model Support

You can pass view models to components:

```csharp
public class CardViewModel
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Footer { get; set; } = "";
}

app.MapGet("/card-vm/{title}", async (string title) =>
{
    var viewModel = new CardViewModel
    {
        Title = title,
        Content = "Content from view model",
        Footer = $"Generated at {DateTime.Now}"
    };
    var html = await CardExtensions.RenderCardFromViewModelAsync(viewModel);
    return Results.Content(html, "text/html");
});
```

## htmx Integration

The demo page (`/demo`) shows how RazorBlade components work perfectly with htmx:

```html
<button hx-get="/card/Welcome" hx-target="#content">
    Load Card
</button>

<div id="content">
    <!-- Component HTML loads here -->
</div>
```

Clicking the button fetches the rendered component HTML and swaps it into the target div - perfect for building modern, dynamic UIs without heavy JavaScript frameworks.

## Key Benefits

- **Type Safety**: Component parameters are checked at compile time
- **No Boilerplate**: No need for MVC controllers, views folders, or Blazor hosting
- **Performance**: Direct component rendering with minimal overhead
- **Flexibility**: Works with any HTTP client, especially great with htmx
- **Developer Experience**: IntelliSense support for all generated methods
- **Layout Support**: Render with or without layouts for full pages or partials
- **View Models**: Easily map view model properties to component parameters
- **Component Composition**: Razor components can include other Razor components
