# RazorBlade Sample Application

This sample demonstrates how to use RazorBlade to render Razor components in minimal API endpoints.

## What's Included

### Components

1. **Greeting.razor** - Simple greeting component with a Name parameter
2. **Card.razor** - Card component with Title, Content, and Footer parameters
3. **ItemList.razor** - List component that displays a collection of items

### Endpoints

All endpoints are defined in `Program.cs`:

- `GET /` - Returns a greeting component
- `GET /card/{title}` - Returns a card with dynamic title and timestamp
- `GET /items` - Returns a shopping list
- `GET /demo` - Returns an interactive htmx demo page

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
- http://localhost:5018/
- http://localhost:5018/demo

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
