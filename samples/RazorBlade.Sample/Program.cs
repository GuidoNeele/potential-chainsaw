using RazorBlade.Sample.Components;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simple endpoint returning a greeting component
app.MapGet("/", () =>
{
    var greeting = new Greeting { Name = "RazorBlade User" };
    return Results.Content(greeting.Render(), "text/html");
});

// Endpoint that returns a card component - perfect for htmx
app.MapGet("/card/{title}", (string title) =>
{
    var card = new Card
    {
        Title = title,
        Content = "This is dynamically generated content for the card.",
        Footer = $"Generated at {DateTime.Now:HH:mm:ss}"
    };
    return Results.Content(card.Render(), "text/html");
});

// Endpoint that returns an item list
app.MapGet("/items", () =>
{
    var itemList = new ItemList
    {
        Title = "My Shopping List",
        Items = new List<string> { "Apples", "Bananas", "Oranges", "Grapes" }
    };
    return Results.Content(itemList.Render(), "text/html");
});

// Demo page as a partial (no layout) - for htmx partial updates
app.MapGet("/demo", () =>
{
    var demoPage = new DemoPage();
    return Results.Content(demoPage.Render(), "text/html");
});

// Demo page with layout
app.MapGet("/demo/full", () =>
{
    var layout = new Layout { Title = "RazorBlade Demo" };
    var demoPage = new DemoPage();
    layout.Body = demoPage;
    return Results.Content(layout.Render(), "text/html");
});

// Example using object initialization
app.MapGet("/card-example/{title}", (string title) =>
{
    var card = new Card
    {
        Title = title,
        Content = "Content from direct instantiation",
        Footer = $"Rendered at {DateTime.Now:HH:mm:ss}"
    };
    return Results.Content(card.Render(), "text/html");
});

app.Run();

