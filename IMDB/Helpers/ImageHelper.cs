using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace IMDB.Helpers
{
    public static class ImageHelper
    {
        public static async Task<string> ProcessImageAsync(IFormFile file, string webRootPath, string folderName)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());

            int size = Math.Min(image.Width, image.Height);

            image.Mutate(x => x
                .Crop(new Rectangle(
                    (image.Width - size) / 2,
                    (image.Height - size) / 2,
                    size,
                    size))
                .Resize(300, 300)
            );

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string folderPath = Path.Combine(webRootPath, "Images", folderName);

            Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);

            await image.SaveAsync(fullPath);

            return $"/Images/{folderName}/{fileName}";
        }

    }
}
