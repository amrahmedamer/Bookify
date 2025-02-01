using Bookify.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace Bookify.Application.Common.Services.Books
{
    internal class BookServices : IBookServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public (int count, IQueryable<Book> book) GetBooks(GetFilteredDto dto)
        {
            IQueryable<Book> books = _unitOfWork.Books
                .GetQueryable()
                .Include(a => a.Author)
                .Include(c => c.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(dto.searchValue))
                books = books.Where(b => b.Title.Contains(dto.searchValue!) || b.Author!.Name.Contains(dto.searchValue!));

            books = books
                .OrderBy($"{dto.sorColumn} {dto.sorColumnDirection}")
                .Skip(dto.skip)
                .Take(dto.sizePage);

            var count = _unitOfWork.Books.Count();

            return (count, books);

        }
    }
}
