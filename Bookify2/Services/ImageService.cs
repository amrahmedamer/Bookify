namespace Bookify.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedExtentions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }



        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        {

            var extension = Path.GetExtension(image.FileName);

            if (!_allowedExtentions.Contains(extension))
                return (isUploaded: false, errorMessage: Errors.NoTAllowedExtention);

            if (image.Length > _maxAllowedSize)
                return (isUploaded: false, errorMessage: Errors.MaxSize);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);

            using var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();


            if (hasThumbnail)
            {
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);
                using var loadedimage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedimage.Width / 200;
                var height = loadedimage.Height / ratio;
                loadedimage.Mutate(i => i.Resize(width: 200, height: (int)height));
                loadedimage.Save(thumbPath);
            }

            return (isUploaded: true, errorMessage: null);
        }

        public void Delete(string ImageUrl, string? ImageThumbnailUrl = null)
        {
            var oldImagePath = $"{_webHostEnvironment.WebRootPath}{ImageUrl}";

            if (File.Exists(oldImagePath))
                File.Delete(oldImagePath);

            if (!string.IsNullOrEmpty(ImageThumbnailUrl))
            {
                var oldThumbImagePath = $"{_webHostEnvironment.WebRootPath}{ImageThumbnailUrl}";

                if (File.Exists(oldThumbImagePath))
                    File.Delete(oldThumbImagePath);
            }
        }
    }
}
