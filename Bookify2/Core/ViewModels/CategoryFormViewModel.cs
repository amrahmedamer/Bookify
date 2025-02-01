namespace Bookify.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int? CategoryId { get; set; }
        [Display(Name = "Categroy")]
        [Remote("AllowItem", null!, AdditionalFields = "CategoryId", ErrorMessage = Errors.Duplicated)]
        public string CategoryName { get; set; } = null!;
    }
}
