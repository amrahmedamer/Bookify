
using Bookify.Application.Common.Interfaces.Repositories;

namespace Bookify.Infrastructure.Persistence.Repositories
{
    public class AuthorReopsitory: IAuthorReopsitory
    {
        private readonly IApplicationDbContext _context;

        public AuthorReopsitory(IApplicationDbContext context)
        {
            _context = context;
        }
        public Author Add(Author author, string CreatedBy)
        {
            author.CreatedById = CreatedBy;
            _context.Authors.Update(author);
            _context.SaveChanges();
            return author;
        }
        public Author Update(Author author, string lastUpdatedById, DateTime lastUpdatedOn)
        {
            author.LastUpdatedById = lastUpdatedById;
            author.LastUpdatedOn = lastUpdatedOn;
            _context.Authors.Update(author);
            _context.SaveChanges();
            return author;
        }

        public Author? Find(int? id) => _context.Authors.Find(id);


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
