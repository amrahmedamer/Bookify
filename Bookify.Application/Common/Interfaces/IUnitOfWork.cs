using Bookify.Domain.Entities;

namespace Bookify.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        public IBaseRepository<Author> Authors { get; }
        public IBaseRepository<BookCopy> BookCopies { get; }
        public IBaseRepository<Book> Books { get; }
        public IBaseRepository<Category> Categories { get; }
        public IBaseRepository<RentalCopy> RentalCopies { get; }
        public IBaseRepository<Subscriber> Subscribers { get; }
        int Complete();
    }
}
