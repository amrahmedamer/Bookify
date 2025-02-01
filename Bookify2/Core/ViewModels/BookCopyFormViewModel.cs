namespace Bookify.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        [Display(Name = "is Available For Rentel")]
        public bool isAvailableForRentel { get; set; }
        [Display(Name = "Edition Number")]
        public int EditionNumber { get; set; }
        public bool ShowRentelInput { get; set; }

    }
}
