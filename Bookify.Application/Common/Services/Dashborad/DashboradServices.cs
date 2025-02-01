using Bookify.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Common.Services.Dashborad
{
    internal class DashboradServices : IDashboradServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboradServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public (int numberOfCopies, int numberOfSubscriber) Count()
        {
            var numberOfCopies = _unitOfWork.BookCopies.Count(c => !c.IsDeleted);
            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;
            var numberOfSubscriber = _unitOfWork.Subscribers.Count(c => !c.IsDeleted);

            return (numberOfCopies, numberOfSubscriber);
        }

        public List<ChartItemDto> GetRentalCity()
        {
            return _unitOfWork.Subscribers
                            .GetQueryable()
                            .Include(s => s.Governorate)
                            .Where(s => !s.IsDeleted)
                            .GroupBy(c => new { c.Governorate!.Name })
                            .Select(r => new ChartItemDto
                            {
                                Label = r.Key.Name,
                                Value = r.Count().ToString()
                            })
                            .ToList();
        }

        public List<ChartItemDto> GetRentalDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            return _unitOfWork.RentalCopies
                .GetQueryable()
                .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                .GroupBy(c => new { Date = c.RentalDate })
                .Select(g => new ChartItemDto
                {
                    Label = g.Key.Date.ToString("d MMM"),
                    Value = g.Count().ToString()
                })
                .ToList();
        }

        public (List<Book> lastAddedBooks, List<BookDto> topBooks) SelectBooks()
        {
            var lastAddedBooks = _unitOfWork.Books
                .GetQueryable()
                 .Include(a => a.Author)
                 .Where(c => !c.IsDeleted)
                 .OrderByDescending(b => b.Id)
                 .Take(8)
                 .ToList();

            var topBooks = _unitOfWork.RentalCopies
                .GetQueryable()
               .Include(c => c.BookCopy)
               .ThenInclude(c => c!.Book)
               .ThenInclude(c => c!.Author)
               .GroupBy(c => new
               {
                   c.BookCopy!.BookId,
                   c.BookCopy!.Book!.Title,
                   c.BookCopy!.Book!.ImageThumbnailUrl,
                   AuthorName = c.BookCopy!.Book!.Author!.Name,
               })
               .Select(c => new
               {
                   c.Key.BookId,
                   c.Key.Title,
                   c.Key.ImageThumbnailUrl,
                   c.Key.AuthorName,
                   count = c.Count()
               })
                .OrderByDescending(b => b.count)
                .Select(c => new BookDto
                {
                    Id = c.BookId,
                    Title = c.Title,
                    ImageThumbnailUrl = c.ImageThumbnailUrl!,
                    Author = c.AuthorName,
                })
               .ToList();

            return (lastAddedBooks, topBooks);
        }
    }
}
