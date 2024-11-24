using System.IO.Compression;
using WebApplication_HW_15_11_2024_ADD.Middleware;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseStaticFiles();

app.UseMiddleware<ImageArchiveMiddleware>();

app.MapGet("/", async (context) =>
{
    var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
    if (!Directory.Exists(imageFolder))
    {
        Directory.CreateDirectory(imageFolder);
    }

    var images = Directory.GetFiles(imageFolder).Select(Path.GetFileName);

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync("<h1>Available Images</h1>");
    await context.Response.WriteAsync("<ul>");
    foreach (var image in images)
    {
        await context.Response.WriteAsync($"<li>{image}</li>");
    }
    await context.Response.WriteAsync("<p>Use <code>/download-images?images=image1.jpg,image2.jpg</code> to download a ZIP file.</p>");
});

app.Run();