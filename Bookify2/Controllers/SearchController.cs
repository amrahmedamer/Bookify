namespace Bookify.Controllers
{
    public class SearchController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IHashids _hashids;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SearchController(IDataProtectionProvider dataProtector, IApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Find(string query)
        {
            var book = _context.Books
                .Include(a => a.Author)
                .Where(b => !b.IsDeleted && (b.Title.Contains(query) || b.Author!.Name.Contains(query)))
                .Select(b => new { b.Title, Author = b.Author!.Name, Key = _hashids.EncodeHex(b.Id.ToString()) })
                .ToList();

            return Ok(book);
        }

        public IActionResult Details(string bKey)
        {
            if (bKey is null)
                return NotFound();

            var BookId = int.Parse(_hashids.DecodeHex(bKey));

            var book = _context.Books
                .Include(c => c.Author)
                .Include(c => c.BookCopies)
                .Include(c => c.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == BookId && !b.IsDeleted);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);


            return View(viewModel);
        }
    }
}
