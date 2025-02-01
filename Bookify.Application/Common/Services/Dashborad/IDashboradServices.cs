namespace Bookify.Application.Common.Services.Dashborad
{
    public interface IDashboradServices
    {
        (List<Book> lastAddedBooks, List<BookDto> topBooks) SelectBooks();
       (int numberOfCopies, int numberOfSubscriber)  Count();
       List<ChartItemDto> GetRentalDay(DateTime? startDate, DateTime? endDate);
       List<ChartItemDto> GetRentalCity();
    }
}
