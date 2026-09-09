using System.Drawing.Printing;
using FoodOrder.PrintAgent;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.WebHost.UseUrls("http://localhost:9100");

builder.Services.AddCors(options =>
{
    options.AddPolicy("PrintAgentCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("PrintAgentCors");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/printers", () =>
{
    var names = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
    return Results.Ok(names);
});

app.MapPost("/print", (PrintRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.PrinterName))
        return Results.BadRequest(new { success = false, error = "printerName is required." });

    try
    {
        var bytes = request.Format?.ToLowerInvariant() == "hex"
            ? Convert.FromHexString(request.Data.Replace(" ", "").Replace("\n", ""))
            : System.Text.Encoding.UTF8.GetBytes(request.Data);

        RawPrinterHelper.SendBytes(request.PrinterName, bytes);
        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { success = false, error = ex.Message });
    }
});

app.Run();

internal record PrintRequest(string PrinterName, string Data, string? Format);
