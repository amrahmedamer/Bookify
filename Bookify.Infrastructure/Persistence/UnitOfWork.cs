using Bookify.Application.Common.Interfaces.Repositories;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Persistence.Repositories;

namespace Bookify.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public IBaseRepository<Author> Authors => new BaseRepository<Author>(_context);
        public IBaseRepository<BookCopy> BookCopies => new BaseRepository<BookCopy>(_context);
        public IBaseRepository<Book> Books => new BaseRepository<Book>(_context);

        public IBaseRepository<Category> Categories => new BaseRepository<Category>(_context);
        public IBaseRepository<RentalCopy> RentalCopies => new BaseRepository<RentalCopy>(_context);
        public IBaseRepository<Subscriber> Subscribers => new BaseRepository<Subscriber>(_context);

        public int Complete()
        {
          return _context.SaveChanges();
        }
    }
}
