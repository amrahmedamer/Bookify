
using Bookify.Domain.Entities;
using System.Linq.Expressions;

namespace Bookify.Application.Common.Interfaces.Repositories
{
    public interface IAuthorReopsitory
    {
        public IEnumerable<Author> GetAll();
        public Author Add(Author entity, string CreatedBy);
        public Author Update(Author author, string lastUpdatedById, DateTime lastUpdatedOn);
        public Author? Find(int? id);
        public Author SingleOrDefault(Expression<Func<Author, bool>> expression);
        public void SaveChanges();
    }
}
