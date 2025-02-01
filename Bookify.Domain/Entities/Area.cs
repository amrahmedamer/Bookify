namespace Bookify.Domain.Entities
{
    public class Area : BaseEntity
    {
        public int AreaId { get; set; }
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
    }
}
