using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }
        [ Display(Name = "First Name") ]
        public string FirstName { get; set; } = null!;
        [ Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFutureDate)]
        public DateTime DateOfBirth { get; set; }
        [ Remote("AllowItemNationalId", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated),Display(Name = "National ID")]
        public string NationalId { get; set; } = null!;
        [ Remote("AllowItemEmail", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [ Display(Name = "Mobile Number"),
        Remote("AllowItemMobileNumber", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string MobileNumber { get; set; } = null!;
        [Display(Name = "Has WhatsApp")]
        public bool HasWhatsApp { get; set; }
        [RequiredIf("Key == ''", ErrorMessage = "Please an select image")]
        public IFormFile? Image { get; set; }
        [Display(Name = "Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Area { get; set; } = new List<SelectListItem>();
        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorate { get; set; }
        public string Address { get; set; } = null!;
        public string? ImagerUrl { get; set; }
        public string? ImagerThumbnailUrl { get; set; }
    }
}
