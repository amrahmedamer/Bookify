using Microsoft.AspNetCore.Localization;

namespace Bookify.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;
        private readonly IHashids _hashids;



        public HomeController(ILogger<HomeController> logger, IApplicationDbContext context, IMapper mapper, IDataProtectionProvider dataProtector, IHashids hashids)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            var lastAddedBooks = _context.Books
              .Include(a => a.Author)
              .Where(c => !c.IsDeleted)
              .OrderByDescending(b => b.Id)
              .Take(10)
              .ToList();


            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);

            foreach (var book in viewModel)
                book.bKey = _hashids.EncodeHex(book!.Id.ToString());

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = 505)
        {
            return View(new ErrorViewModel { ErrorCode = statusCode, ErrorDescription = ReasonPhrases.GetReasonPhrase(statusCode) });
        }
    }
}