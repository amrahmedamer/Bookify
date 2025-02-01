namespace Bookify.Domain.Entities
{
    public class Author : BaseEntity
    {
        public int id { get; set; }
        public string Name { get; set; } = null!;
    }
}
