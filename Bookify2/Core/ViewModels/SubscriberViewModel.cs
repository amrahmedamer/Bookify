namespace Bookify.Core.ViewModels
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
        public bool HasWhatsApp { get; set; }
        public string? Email { get; set; }
        public string? ImagerUrl { get; set; }
        public string? ImagerThumbnailUrl { get; set; }
        public string? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Address { get; set; }
        public bool IsBlockListed { get; set; }
        public DateTime CreatedOn { get; set; }
        public IEnumerable<subscriptionViewModel> Subscriptions { get; set; } = new List<subscriptionViewModel>();
        public IEnumerable<RentalViewModel> Rentals { get; set; } = new List<RentalViewModel>();

    }
}
