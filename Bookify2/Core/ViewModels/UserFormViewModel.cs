using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }
        [ Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;
        [Remote("AllowUserName", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [ Display(Name = "Username")]
        public string UserName { get; set; } = null!;

        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;
        [ DataType(DataType.Password), RequiredIf("Id==null", ErrorMessage = Errors.Required)]
        public string? Password { get; set; } = null!;
        [Display(Name = "Confirm password"), DataType(DataType.Password), 
            RequiredIf("Id==null", ErrorMessage = Errors.Required)]
        public string? ConfirmPassword { get; set; } = null!;
        [Display(Name = "Roles")]
        public IList<string> SelectRoles { get; set; } = new List<string>();
        public IEnumerable<SelectListItem>? Roles { get; set; }

    }
}
