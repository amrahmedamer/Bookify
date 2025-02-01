namespace Bookify.Services
{
    public interface IImageService
    {
        Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool thumbnail);
        void Delete(string ImageUrl, string? ImageThumbnailUrl = null);

    }
}
