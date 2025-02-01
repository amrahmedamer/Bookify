namespace Bookify.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int? id { get; set; }
        [ Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
