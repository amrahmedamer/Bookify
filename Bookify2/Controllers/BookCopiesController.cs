
namespace Bookify.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<BookCopyFormViewModel> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public BookCopiesController(IMapper mapper, IValidator<BookCopyFormViewModel> validator, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }
       
        [AjaxOnly]
        public IActionResult Create(int BookId)
        {
            var book = _unitOfWork.Books.GetById(BookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = BookId,
                ShowRentelInput = book.isAvailableForRentel

            };

            return PartialView("Form", viewModel);
        }
        [HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                return PartialView("Form", model);

            var book = _unitOfWork.Books.GetById(model.BookId);

            if (book is null)
                return NotFound();

            var Copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                isAvailableForRentel = book.isAvailableForRentel ? model.isAvailableForRentel : false,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value

            };

            book.BookCopies.Add(Copy);
            _unitOfWork.Complete();

            var viewModel = _mapper.Map<BookCopyViewModel>(Copy);

            return PartialView("_BookCopyRow", viewModel);
        }
        public IActionResult Edit(int id)
        {
            var bookCopy = _unitOfWork.BookCopies.Find(x => x.Id == id, b => b.Include(b => b.Book)!);
            //_context.BookCopies.Include(b => b.Book).SingleOrDefault(b => b.Id == id);

            if (bookCopy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(bookCopy);
            viewModel.ShowRentelInput = bookCopy.Book!.isAvailableForRentel;

            return PartialView("Form", viewModel);
        }
        [HttpPost]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                return PartialView("Form", model);

            var bookCopy = _unitOfWork.BookCopies.Find(x => x.Id == model.Id, b => b.Include(b => b.Book)!);

            if (bookCopy is null)
                return NotFound();

            bookCopy.EditionNumber = model.EditionNumber;
            bookCopy.isAvailableForRentel = bookCopy.Book!.isAvailableForRentel ? model.isAvailableForRentel : false;
            bookCopy.LastUpdatedById = User.GetUserId();
            bookCopy.LastUpdatedOn = DateTime.UtcNow;

            _unitOfWork.Complete();

            var viewModel = _mapper.Map<BookCopyViewModel>(bookCopy);

            return PartialView("_BookCopyRow", viewModel);
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var bookCopy = _unitOfWork.BookCopies.GetById(id);

            if (bookCopy is null)
                return NotFound();

            bookCopy.IsDeleted = !bookCopy.IsDeleted;
            bookCopy.LastUpdatedById = User.GetUserId();
            bookCopy.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            return Ok();
        }

        public IActionResult RentalHistory(int id)
        {
            var copy = _unitOfWork.RentalCopies.FindAll(
                predicate: r => r.BookCopyId == id,
                include: c => c.Include(c => c.Rental).ThenInclude(c => c!.Subscriber)!,
                orderBy: r => r.RentalDate,
                orderByDirection: OrderBy.Descending
                );
            // _context.RentalCopies
            //.Include(c => c.Rental)
            //.ThenInclude(c => c!.Subscriber)
            //.Where(r => r.BookCopyId == id)
            //.ToList();

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(copy);

            return View(viewModel);
        }
    }
}
