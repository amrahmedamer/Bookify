
namespace Bookify.Domain.Entities
{
    public class Category : BaseEntity
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();
    }
}
