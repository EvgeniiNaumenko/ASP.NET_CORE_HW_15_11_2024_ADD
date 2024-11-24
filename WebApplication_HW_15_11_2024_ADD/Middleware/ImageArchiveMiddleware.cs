using System.IO.Compression;

namespace WebApplication_HW_15_11_2024_ADD.Middleware
{
    public class ImageArchiveMiddleware
    {
        private readonly RequestDelegate _next;

        public ImageArchiveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/download-images")
            {
                var imageNames = context.Request.Query["images"].ToString().Split(',');
                var images = imageNames.Select(name => Path.Combine("wwwroot", "files", name)).ToList();

                if (images.Any(file => !File.Exists(file)))
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Some images not found.");
                    return;
                }

                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

                try
                {
                    using (var zip = ZipFile.Open(tempFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var image in images)
                        {
                            zip.CreateEntryFromFile(image, Path.GetFileName(image));
                        }
                    }

                    context.Response.ContentType = "application/zip";
                    context.Response.Headers.Add("Content-Disposition", "attachment; filename=images.zip");
                    await using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        await fileStream.CopyToAsync(context.Response.Body);
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Error occurred: {ex.Message}");
                }
                finally
                {
                    if (File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch {}
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

}

