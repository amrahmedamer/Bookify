using Bookify.Application.Common.Dto;

namespace Bookify.Application.Common.Services.Books
{
    public interface IBookServices
    {
        (int count, IQueryable<Book> book) GetBooks(GetFilteredDto dto);
    }
}
