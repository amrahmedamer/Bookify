using Bookify.Core.Utilities;

namespace Bookify.Core.ViewModels
{
    public class RentalsReportViewModel
    {
        public string duration { get; set; }
        public PaginatedList<RentalCopy> rentals { get; set; }
    }
}
