namespace Bookify.Domain.Entities
{
    public class Governorate : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Area> Books { get; set; } = new List<Area>();
    }
}
