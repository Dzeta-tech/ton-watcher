using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/webhook", async (HttpRequest request) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        
        Console.WriteLine("=== WEBHOOK RECEIVED ===");
        Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"Content-Type: {request.ContentType}");
        
        if (request.Headers.Authorization.Any())
        {
            Console.WriteLine($"Authorization: {request.Headers.Authorization.First()}");
        }
        
        Console.WriteLine("Raw Body:");
        Console.WriteLine(body);
        
        // Try to parse and pretty print JSON
        try
        {
            var jsonDoc = JsonDocument.Parse(body);
            var prettyJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            Console.WriteLine("\nParsed JSON:");
            Console.WriteLine(prettyJson);
        }
        catch (JsonException)
        {
            Console.WriteLine("Body is not valid JSON");
        }
        
        Console.WriteLine("========================\n");
        
        return Results.Ok(new { status = "received", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing webhook: {ex.Message}");
        return Results.StatusCode(500);
    }
});

app.MapGet("/", () => "Webhook Listener is running! Send POST requests to /webhook");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = $"http://0.0.0.0:{port}";

Console.WriteLine($"Starting webhook listener on {url}");
Console.WriteLine("Waiting for webhooks...\n");

app.Run(url); 