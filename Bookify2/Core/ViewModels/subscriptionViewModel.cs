namespace Bookify.Core.ViewModels
{
    public class subscriptionViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status
        {
            get
            {
                return DateTime.Today > EndDate ? SubscriptionStatus.Expierd : DateTime.Today < StartDate ? string.Empty : SubscriptionStatus.Active;
            }
        }

    }
}
