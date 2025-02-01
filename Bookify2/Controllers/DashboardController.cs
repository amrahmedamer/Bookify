using Bookify.Application.Common.Services.Dashborad;

namespace Bookify.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IDashboradServices _dashboradServices;
        private readonly ApplicationDbContext _context;
        public DashboardController(IMapper mapper, IDashboradServices dashboradServices, ApplicationDbContext context)
        {
            _mapper = mapper;
            _dashboradServices = dashboradServices;
            _context = context;
        }

        public IActionResult Index()
        {
           
            var(numberOfCopies, numberOfSubscriber) = _dashboradServices.Count();
            var(lastAddedBooks, topBooks) = _dashboradServices.SelectBooks();

            var viewModel = new DashboardViewModel()
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscriber = numberOfSubscriber,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
                TopBooks = _mapper.Map<IEnumerable<BookViewModel>>(topBooks)
            };
            return View(viewModel);
        }
        [AjaxOnly]
        public IActionResult GetRentalPerDay(DateTime? startDate, DateTime? endDate)=> 
            Ok(_dashboradServices.GetRentalDay(startDate, endDate));

        public IActionResult GetRentalPerCity()=> Ok(_dashboradServices.GetRentalCity());

    }
}
