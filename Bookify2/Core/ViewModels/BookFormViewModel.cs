using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }
        [Remote("AllowItem", null!, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DuplicatedBook)]
        public string Title { get; set; } = null!;
        [Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.DuplicatedBook)]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Author { get; set; }
        public string Publisher { get; set; } = null!;
        [Display(Name = "Puplishing Date")]
        [AssertThat("PuplishingDate <= Today()", ErrorMessage = Errors.NotAllowFutureDate)]
        public DateTime PuplishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string Hall { get; set; } = null!;
        [Display(Name = "Is Available For Rentel?")]
        public bool isAvailableForRentel { get; set; }
        public string Description { get; set; } = null!;
        [Display(Name = "Categories")]
        public IList<int> SelectCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? Categories { get; set; }


    }
}
