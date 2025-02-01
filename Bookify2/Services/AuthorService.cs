

using CloudinaryDotNet.Actions;
using System.Linq.Expressions;

namespace Bookify.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IApplicationDbContext _context;

        public AuthorService(IApplicationDbContext context)
        {
            _context = context;
        }

        public Author Add(Author author, string CreatedBy)
        {
            author.CreatedById= CreatedBy;
            _context.Authors.Update(author);
            _context.SaveChanges();
            return author;
        }
        public Author Update(Author author , string lastUpdatedById, DateTime lastUpdatedOn)
        {
            author.LastUpdatedById= lastUpdatedById;
            author.LastUpdatedOn= lastUpdatedOn;
            _context.Authors.Update(author);
            _context.SaveChanges();
            return author;
        }

        public Author? Find(int id) => _context.Authors.Find(id);


        public IEnumerable<Author> GetAll()
        {
            return _context.Authors.AsNoTracking().ToList();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public Author SingleOrDefault(Expression<Func<Author, bool>> expression)
        {
            var author = _context.Authors.SingleOrDefault(expression);
            return author;
        }
    }
}
