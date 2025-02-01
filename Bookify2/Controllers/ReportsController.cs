using Bookify.Core.Utilities;
using Bookify.Web.Core.Enums;
using ClosedXML.Excel;
using OpenHtmlToPdf;
using ViewToHTML.Services;

namespace Bookify.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IViewRendererService _viewRendererService;
        private int _sheetStartRow = 5;
        private readonly string _logoPath;


        public ReportsController(IApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment, IViewRendererService viewRendererService)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _viewRendererService = viewRendererService;
            _logoPath = $"{_webHostEnvironment.WebRootPath}/assets/images/logo.png";
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Books
        public IActionResult Books(IList<int> SelectedAuthors, IList<int> SelectedCategories, int? PageNumber)
        {
            var authors = _context.Authors.OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.OrderBy(a => a.CategoryName).ToList();

            IQueryable<Book> books = _context.Books
                .Include(a => a.Author)
                .Include(c => c.Categories)
                .ThenInclude(c => c.Category)
                .Where(a => (!SelectedAuthors.Any() || SelectedAuthors.Contains(a.AuthorId))
                && (!SelectedCategories.Any() || a.Categories.Any(c => SelectedCategories.Contains(c.CategoryId))));

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (PageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(books, PageNumber ?? 0, (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }

        public async Task<IActionResult> ExportBooksToFile(string authors, string categories)
        {
            var SelectedAuthors = authors?.Split(',');
            var SelectedCategories = categories?.Split(',');


            var books = _context.Books
                .Include(a => a.Author)
                .Include(c => c.Categories)
                .ThenInclude(c => c.Category)
                .Where(a => (string.IsNullOrEmpty(authors) || SelectedAuthors!.Contains(a.AuthorId.ToString()))
                && (string.IsNullOrEmpty(categories) || a.Categories.Any(c => SelectedCategories!.Contains(c.CategoryId.ToString()))))
                .ToList();



            using var workbook = new XLWorkbook();
            var sheet = workbook.AddWorksheet("Books");

            sheet.AddImage(_logoPath);
            var headerCells = new string[] { "Title", "Author", "Categories", "Publisher", "Publishing Date", "Hall", "Available For Rental", "status" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < books.Count(); i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(books[i].Title);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(books[i].Author!.Name);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(string.Join(", ", books[i].Categories.Select(c => c.Category!.CategoryName)));
                sheet.Cell(i + _sheetStartRow, 4).SetValue(books[i].Publisher);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(books[i].PuplishingDate.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 6).SetValue(books[i].Hall);
                sheet.Cell(i + _sheetStartRow, 7).SetValue(books[i].isAvailableForRentel ? "Yes" : "No");
                sheet.Cell(i + _sheetStartRow, 8).SetValue(books[i].IsDeleted ? "Deleted" : "Available");
            }

            sheet.Format();
            sheet.AddTable(books.Count, headerCells.Length);
            sheet.ShowGridLines = false;


            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);


            return File(stream.ToArray(), "application/octet-stream", "Books.xlsx");
        }
        public async Task<IActionResult> ExportBooksToPDF(string authors, string categories)
        {
            var SelectedAuthors = authors?.Split(',');
            var SelectedCategories = categories?.Split(',');


            var books = _context.Books
                .Include(a => a.Author)
                .Include(c => c.Categories)
                .ThenInclude(c => c.Category)
                .Where(a => (string.IsNullOrEmpty(authors) || SelectedAuthors!.Contains(a.AuthorId.ToString()))
                && (string.IsNullOrEmpty(categories) || a.Categories.Any(c => SelectedCategories!.Contains(c.CategoryId.ToString()))))
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);

            var templatePath = "~/Views/Reports/BooksTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), "application/octet-stream", "Books.pdf");
        }
        #endregion

        #region Rentals
        public IActionResult Rental(RentalsReportViewModel model, int? PageNumber)
        {

            if (!string.IsNullOrEmpty(model.duration))
            {

                var dates = model.duration.Split('-');
                var isValidFormateStartDate = DateTime.TryParse(dates[0].Trim(), out DateTime from);
                var isValidFormateEndDate = DateTime.TryParse(dates[1].Trim(), out DateTime to);

                if (!isValidFormateStartDate)
                {
                    ModelState.AddModelError("duration", Errors.InvalidStartDate);
                    return View(model);
                }
                if (!isValidFormateEndDate)
                {
                    ModelState.AddModelError("duration", Errors.InvalidEndDate);
                    return View(model);
                }

                IQueryable<RentalCopy> Rentals = _context.RentalCopies
                    .Include(r => r.BookCopy)
                    .ThenInclude(b => b!.Book)
                    .ThenInclude(a => a!.Author)
                    .Include(r => r.Rental)
                    .ThenInclude(s => s!.Subscriber)
                    .Where(r => r.RentalDate >= from && r.RentalDate <= to);

                if (PageNumber is not null)
                    model.rentals = PaginatedList<RentalCopy>.Create(Rentals, PageNumber ?? 1, (int)ReportsConfigurations.PageSize);
            }
            ModelState.Clear();
            return View(model);
        }

        public IActionResult ExportRentalsToExcel(string duration)
        {
            var dates = duration.Split('-');
            var from = DateTime.Parse(dates[0].Trim());
            var to = DateTime.Parse(dates[1].Trim());

            var Rentals = _context.RentalCopies
                    .Include(r => r.BookCopy)
                    .ThenInclude(b => b!.Book)
                    .ThenInclude(a => a!.Author)
                    .Include(r => r.Rental)
                    .ThenInclude(s => s!.Subscriber)
                    .Where(r => r.RentalDate >= from && r.RentalDate <= to)
                     .ToList();

            var data = _mapper.Map<IList<RentalCopyViewModel>>(Rentals);

            var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("Rental");

            sheet.AddImage(_logoPath);

            string[] cellsHeader = { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title", "Book Author", "Rental Date", "End Date", "Return Date", "Extended On" };
            sheet.AddHeader(cellsHeader);

            for (int i = 0; i < Rentals.Count(); i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(data[i].Rental!.SubscriberId);
                sheet.Cell(i + _sheetStartRow, 2).SetValue($"{data[i].Rental!.Subscriber!.FirstName} {data[i].Rental!.Subscriber!.LastName}");
                sheet.Cell(i + _sheetStartRow, 3).SetValue(data[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(data[i].BookCopy!.Book!.Title);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(data[i].BookCopy!.Book!.Author!.Name);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(data[i].RentalDate.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 7).SetValue(data[i].EndDate.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(data[i].ReturnDate?.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(data[i].ExtendedOn?.ToString("dd MMM,yyyy"));
            }

            sheet.Format();
            sheet.AddTable(data.Count, cellsHeader.Length);
            sheet.ShowGridLines = false;

            using var stream = new MemoryStream();
            workBook.SaveAs(stream);

            return File(stream.ToArray(), "application/actet-stream", "Rental.xlsx");
        }

        public async Task<IActionResult> ExportRentalsToPDF(string duration)
        {
            var dates = duration.Split('-');
            var from = DateTime.Parse(dates[0].Trim());
            var to = DateTime.Parse(dates[1].Trim());

            var Rentals = _context.RentalCopies
                    .Include(r => r.BookCopy)
                    .ThenInclude(b => b!.Book)
                    .ThenInclude(a => a!.Author)
                    .Include(r => r.Rental)
                    .ThenInclude(s => s!.Subscriber)
                    .Where(r => r.RentalDate >= from && r.RentalDate <= to)
                     .ToList();

            var viewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(Rentals);

            var templatePath = "~/Views/Reports/RentalTemplatet.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();


            return File(pdf.ToArray(), "application/actet-stream", "Rental.pdf");
        }

        #endregion 

        #region Delayed Rental
        public IActionResult DelayedRental()
        {


            IQueryable<RentalCopy> Rentals = _context.RentalCopies
                .Include(r => r.BookCopy)
                .ThenInclude(b => b!.Book)
                .ThenInclude(a => a!.Author)
                .Include(r => r.Rental)
                .ThenInclude(s => s!.Subscriber)
                .Where(c => !c.ReturnDate.HasValue && DateTime.Today > c.EndDate);

            var ViewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(Rentals);

            return View(ViewModel);
        }

        public IActionResult ExportDelayedRentalToExcel()
        {
            var Rentals = _context.RentalCopies
                 .Include(r => r.BookCopy)
                 .ThenInclude(b => b!.Book)
                 .ThenInclude(a => a!.Author)
                 .Include(r => r.Rental)
                 .ThenInclude(s => s!.Subscriber)
                 .Where(c => DateTime.Now > c.EndDate)
                      .ToList();

            var data = _mapper.Map<IList<RentalCopyViewModel>>(Rentals);

            var workBook = new XLWorkbook();
            var sheet = workBook.AddWorksheet("Rental");

            sheet.AddImage(_logoPath);


            string[] cellsHeader = { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title", "Book Serial", "Rental Date", "End Date", "Extended On", "Delay in Days" };


            for (int i = 0; i < cellsHeader.Length; i++)
            {
                sheet.Cell(1, i + 1).SetValue(cellsHeader[i]);
            }

            sheet.AddHeader(cellsHeader);

            for (int i = 0; i < data.Count(); i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(data[i].Rental!.SubscriberId);
                sheet.Cell(i + _sheetStartRow, 2).SetValue($"{data[i].Rental!.Subscriber!.FirstName} {data[i].Rental!.Subscriber!.LastName}");
                sheet.Cell(i + _sheetStartRow, 3).SetValue(data[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(data[i].BookCopy!.Book!.Title);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(data[i].BookCopy!.SerialNumber);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(data[i].RentalDate.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 7).SetValue(data[i].EndDate.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(data[i].ExtendedOn?.ToString("dd MMM,yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(data[i].DelayInDays);
            }

            sheet.Format();
            sheet.AddTable(data.Count, cellsHeader.Length);
            sheet.ShowGridLines = false;

            using var stream = new MemoryStream();
            workBook.SaveAs(stream);

            return File(stream.ToArray(), "application/actet-stream", "DelayedRental.xlsx");
        }

        public async Task<IActionResult> ExportDelayedRentalToPDF()
        {
            var Rentals = _context.RentalCopies
                 .Include(r => r.BookCopy)
                 .ThenInclude(b => b!.Book)
                 .ThenInclude(a => a!.Author)
                 .Include(r => r.Rental)
                 .ThenInclude(s => s!.Subscriber)
                 .Where(c => DateTime.Now > c.EndDate)
                      .ToList();

            var viewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(Rentals);

            var templatePath = "~/Views/Reports/DelayedRentalTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

            var pdf = Pdf.From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();


            return File(pdf.ToArray(), "application/actet-stream", "DelayedRental.pdf");
        }

        #endregion

    }
}
