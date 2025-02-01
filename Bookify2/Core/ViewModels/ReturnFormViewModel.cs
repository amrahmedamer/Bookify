using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Core.ViewModels
{
    public class ReturnFormViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Penalty Paid?"),
            AssertThat("(TotatDelayInDays == 0 && PenaltyPaid == false) || PenaltyPaid == true", ErrorMessage = Errors.PenaltyPaid)]
        public bool PenaltyPaid { get; set; }
        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();
        public List<ReturnCopuViewModel> SelectedCopies { get; set; } = new();
        public bool AllowExtend { get; set; }
        public int TotatDelayInDays
        {
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}
