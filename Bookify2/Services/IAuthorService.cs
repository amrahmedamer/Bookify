using System.Linq.Expressions;

namespace Bookify.Services
{
    public interface IAuthorService
    {
        public IEnumerable<Author> GetAll();
        public Author Add(Author entity,string CreatedBy);
        public Author Update(Author author, string lastUpdatedById, DateTime lastUpdatedOn);
        public Author? Find(int id);
        public Author SingleOrDefault(Expression<Func<Author, bool>> expression);
        public void SaveChanges();
        // public Author Find(Author);
    }
}
