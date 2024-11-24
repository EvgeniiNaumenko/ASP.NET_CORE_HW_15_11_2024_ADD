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
            var request = context.Request;
            var response = context.Response;

            if (request.Path == "/download-images" && request.Query.ContainsKey("images"))
            {
                var imageNames = request.Query["images"].ToString().Split(',');

                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

                var imagePaths = new List<string>();
                foreach (var imageName in imageNames)
                {
                    var imagePath = Path.Combine(imageFolder, imageName);
                    if (File.Exists(imagePath))
                    {
                        imagePaths.Add(imagePath);
                    }
                }

                if (imagePaths.Count == 0)
                {
                    response.StatusCode = 404;
                    await response.WriteAsync("No valid images found.");
                    return;
                }

                var tempZipPath = Path.GetTempFileName() + ".zip";

                using (var zipArchive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                {
                    foreach (var imagePath in imagePaths)
                    {
                        zipArchive.CreateEntryFromFile(imagePath, Path.GetFileName(imagePath));
                    }
                }

                response.ContentType = "application/zip";
                response.Headers.Add("Content-Disposition", "attachment; filename=images.zip");

                await using var fileStream = File.OpenRead(tempZipPath);
                await fileStream.CopyToAsync(response.Body);

                File.Delete(tempZipPath);
                return;
            }


            await _next(context);
        }
    }
}

