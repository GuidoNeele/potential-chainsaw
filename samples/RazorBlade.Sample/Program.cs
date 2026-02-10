using RazorBlade.Extensions;

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

// htmx demo page
app.MapGet("/demo", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>RazorBlade Demo</title>
    <script src="https://unpkg.com/htmx.org@1.9.10"></script>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }
        button { margin: 10px 5px; padding: 10px 20px; cursor: pointer; }
        #content { margin-top: 20px; }
    </style>
</head>
<body>
    <h1>RazorBlade + htmx Demo</h1>
    <p>Click buttons to load partial HTML from Razor components:</p>
    
    <button hx-get="/card/Welcome" hx-target="#content" hx-swap="innerHTML">
        Load Welcome Card
    </button>
    
    <button hx-get="/card/Features" hx-target="#content" hx-swap="innerHTML">
        Load Features Card
    </button>
    
    <button hx-get="/items" hx-target="#content" hx-swap="innerHTML">
        Load Item List
    </button>
    
    <button hx-get="/" hx-target="#content" hx-swap="innerHTML">
        Load Greeting
    </button>
    
    <div id="content">
        <p>Click a button above to load content...</p>
    </div>
</body>
</html>
""", "text/html"));

app.Run();


