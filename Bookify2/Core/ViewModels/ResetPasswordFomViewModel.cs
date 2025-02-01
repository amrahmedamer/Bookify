namespace Bookify.Core.ViewModels
{
    public class ResetPasswordFomViewModel
    {
        public string Id { get; set; } = null!;
        [ DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Display(Name = "Confirm password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
