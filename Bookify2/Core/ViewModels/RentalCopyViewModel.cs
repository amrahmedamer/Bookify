namespace Bookify.Core.ViewModels
{
    public class RentalCopyViewModel
    {
        public BookCopy? BookCopy { get; set; }
        public Rental? Rental { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
        public int DelayInDays
        {
            get
            {
                var delay = 0;

                if (ReturnDate.HasValue && ReturnDate > EndDate)
                    delay = (int)(ReturnDate.Value - EndDate).TotalDays;
                else if (!ReturnDate.HasValue && DateTime.Now > EndDate)
                    delay = (int)(DateTime.Now - EndDate).TotalDays;

                return delay;
            }
        }
    }
}
