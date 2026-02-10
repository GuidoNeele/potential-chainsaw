using RazorBlade.Extensions;
using RazorBlade.Sample.Components;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simple endpoint returning a greeting component
app.MapGet("/", async () =>
{
    var html = await GreetingExtensions.RenderGreetingAsync("RazorBlade User");
    return Results.Content(html, "text/html");
});

// Endpoint that returns a card component - perfect for htmx
app.MapGet("/card/{title}", async (string title) =>
{
    var html = await CardExtensions.RenderCardAsync(
        title, 
        "This is dynamically generated content for the card.",
        $"Generated at {DateTime.Now:HH:mm:ss}");
    return Results.Content(html, "text/html");
});

// Endpoint that returns an item list
app.MapGet("/items", async () =>
{
    var items = new List<string> { "Apples", "Bananas", "Oranges", "Grapes" };
    var html = await ItemListExtensions.RenderItemListAsync("My Shopping List", items);
    return Results.Content(html, "text/html");
});

// Demo page as a Razor component with layout
app.MapGet("/demo", async () =>
{
    var layoutParams = new Dictionary<string, object?>
    {
        { "Title", "RazorBlade Demo" }
    };
    var html = await DemoPageExtensions.RenderDemoPageWithLayoutAsync<Layout>(layoutParams);
    return Results.Content(html, "text/html");
});

// Demo page as a partial (no layout) - for htmx partial updates
app.MapGet("/demo/partial", async () =>
{
    var html = await DemoPageExtensions.RenderDemoPageAsync();
    return Results.Content(html, "text/html");
});

// Example using a view model
app.MapGet("/card-vm/{title}", async (string title) =>
{
    var viewModel = new CardViewModel
    {
        Title = title,
        Content = "Content from view model",
        Footer = $"View model rendered at {DateTime.Now:HH:mm:ss}"
    };
    var html = await CardExtensions.RenderCardFromViewModelAsync(viewModel);
    return Results.Content(html, "text/html");
});

app.Run();

// Example view model class
public class CardViewModel
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string Footer { get; set; } = "";
}

