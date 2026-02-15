# RazorBlade

A lightweight .NET library for rendering Razor components to HTML strings in minimal API endpoints, perfect for building type-safe, MVC-free applications with htmx.

## Overview

RazorBlade takes Razor components and generates strongly-typed extension methods that allow you to render them to HTML strings. This approach removes the need for full MVC or Blazor boilerplate while maintaining type safety and leveraging the power of Razor components.

## Features

- ✨ **Source Generator**: Automatically generates type-safe extension methods from your Razor components
- 🎯 **Type-Safe**: Compile-time checking of component parameters
- 🚀 **Minimal API Ready**: Perfect for .NET minimal APIs
- 🔥 **htmx Compatible**: Return partial HTML views with ease
- 🪶 **Lightweight**: No MVC or Blazor hosting overhead
- ⚡ **Fast**: Direct component rendering without unnecessary middleware
- 🎨 **Layout Support**: Optional layouts for full pages or partials
- 📦 **View Models**: Map view model properties to component parameters

## Getting Started

### Installation

Add the following project references to your web application:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/RazorBlade.Library/RazorBlade.Library.csproj" />
  <ProjectReference Include="path/to/RazorBlade.SourceGenerator/RazorBlade.SourceGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Create a Razor Component

Create a `.razor` file in your project (e.g., `Components/Greeting.razor`):

```razor
@* Greeting.razor *@

<div class="greeting">
    <h1>Hello, @Name!</h1>
    <p>Welcome to RazorBlade components.</p>
</div>

@code {
    [Parameter]
    public string Name { get; set; } = "World";
}
```

### Use in Minimal API

The source generator automatically creates extension methods for your components:

```csharp
using RazorBlade.Extensions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Render component to HTML string
app.MapGet("/greeting/{name}", async (string name) =>
{
    var html = await GreetingExtensions.RenderGreetingAsync(name);
    return Results.Content(html, "text/html");
});

app.Run();
```

## Examples

### Basic Card Component

```razor
@* Card.razor *@

<div class="card">
    <h2>@Title</h2>
    <div class="card-body">@Content</div>
</div>

@code {
    [Parameter]
    public string Title { get; set; } = "Card Title";
    
    [Parameter]
    public string Content { get; set; } = "Content here";
}
```

Usage in minimal API:

```csharp
app.MapGet("/card", async () =>
{
    var html = await CardExtensions.RenderCardAsync(
        "Welcome", 
        "This is dynamic content!");
    return Results.Content(html, "text/html");
});
```

### List Component

```razor
@* ItemList.razor *@

<div class="item-list">
    <h3>@Title</h3>
    <ul>
        @foreach (var item in Items)
        {
            <li>@item</li>
        }
    </ul>
</div>

@code {
    [Parameter]
    public string Title { get; set; } = "Items";
    
    [Parameter]
    public List<string> Items { get; set; } = new();
}
```

Usage:

```csharp
app.MapGet("/items", async () =>
{
    var items = new List<string> { "Apples", "Bananas", "Oranges" };
    var html = await ItemListExtensions.RenderItemListAsync("Shopping List", items);
    return Results.Content(html, "text/html");
});
```

### Layouts and Full Page Rendering

Create a layout component for wrapping content:

```razor
@* Layout.razor *@

<!DOCTYPE html>
<html>
<head>
    <title>@Title</title>
</head>
<body>
    @Body
</body>
</html>

@code {
    [Parameter]
    public string Title { get; set; } = "My App";
    
    [Parameter]
    public RenderFragment Body { get; set; } = null!;
}
```

Render a component with layout:

```csharp
app.MapGet("/page", async () =>
{
    var layoutParams = new Dictionary<string, object?> { { "Title", "My Page" } };
    var html = await DemoPageExtensions.RenderDemoPageWithLayoutAsync<Layout>(layoutParams);
    return Results.Content(html, "text/html");
});
```

Or render as a partial (no layout) for htmx:

```csharp
app.MapGet("/page/partial", async () =>
{
    var html = await DemoPageExtensions.RenderDemoPageAsync();
    return Results.Content(html, "text/html");
});
```

### View Model Support

Use view models to pass parameters to components:

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

### htmx Integration

RazorBlade works perfectly with htmx for dynamic partial page updates:

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://unpkg.com/htmx.org@1.9.10"></script>
</head>
<body>
    <button hx-get="/card/Welcome" hx-target="#content">
        Load Card
    </button>
    
    <div id="content">
        <!-- Component HTML will be loaded here -->
    </div>
</body>
</html>
```

## How It Works

1. **Write Razor Components**: Create standard Razor components with `[Parameter]` attributes
2. **Source Generator**: The RazorBlade source generator scans your `.razor` files during compilation
3. **Generate Extensions**: For each component, it generates strongly-typed extension methods in the `RazorBlade.Extensions` namespace
4. **Render to HTML**: Call the generated methods to render components to HTML strings

### Generated Code Example

For a component like `Greeting.razor`, RazorBlade generates:

```csharp
namespace RazorBlade.Extensions
{
    public static class GreetingExtensions
    {
        public static async Task<string> RenderGreetingAsync(string name)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "Name", name },
            };
            
            return await RazorBlade.ComponentRenderer
                .RenderComponentAsync<RazorBlade.Sample.Components.Greeting>(parameters);
        }
        
        public static string RenderGreeting(string name)
        {
            return RenderGreetingAsync(name).GetAwaiter().GetResult();
        }
    }
}
```

## Project Structure

```
RazorBlade/
├── src/
│   ├── RazorBlade.Library/          # Runtime library
│   │   ├── RazorComponent.cs        # Base component class
│   │   ├── ComponentRenderer.cs     # HTML renderer
│   │   └── GenerateComponentAttribute.cs
│   └── RazorBlade.SourceGenerator/  # Source generator
│       └── RazorComponentGenerator.cs
├── samples/
│   └── RazorBlade.Sample/           # Demo application
│       ├── Components/              # Sample components
│       └── Program.cs               # Minimal API endpoints
└── README.md
```

## Running the Sample

```bash
cd samples/RazorBlade.Sample
dotnet run
```

Navigate to:
- `http://localhost:5018/` - Greeting component
- `http://localhost:5018/card/YourTitle` - Card component with dynamic title
- `http://localhost:5018/items` - Shopping list component
- `http://localhost:5018/demo` - Interactive htmx demo

Or run with HTTPS:
- `https://localhost:7220/` - Same endpoints over HTTPS

## Demo Screenshot

![RazorBlade Demo](https://github.com/user-attachments/assets/2e3aacad-bb64-4e0b-9b5d-94d68ebbaeb7)

## Comparison with Razorblade Library

While inspired by the Razorblade library, this project focuses specifically on:
- **Razor Components** instead of Razor Pages
- **Source generation** for type-safe APIs
- **Minimal API** patterns instead of MVC
- **htmx-first** approach for partial HTML rendering

## Requirements

- .NET 9.0 or later
- Razor components support

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

[Your License Here]

## Acknowledgments

Inspired by the [Razorblade](https://github.com/ltrzesniewski/RazorBlade) library by Lucas Trzesniewski.
