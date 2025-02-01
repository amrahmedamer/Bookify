namespace Bookify.Core.ViewModels
{
    public class BookCopyViewModel
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = null!;
        public int BookId { get; set; }
        public string? BookThumbnailUrl { get; set; }
        public bool isAvailableForRentel { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
