namespace Bookify.Controllers
{
    public class RentalsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(IApplicationDbContext context, IDataProtectionProvider dataProtector, IMapper mapper)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");

            _mapper = mapper;
        }

        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = validateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);


            var viewModel = new RentalFormViewModel()
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };
            return View("Form", viewModel);
        }
        [HttpPost]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = validateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalError, copies) = validdateCopies(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalError))
                return View("NotAllowedRental", rentalError);

            Rental rental = new()
            {
                RentalCopies = copies!,
                CreatedById = User.GetUserId()
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals
               .Include(r => r.RentalCopies)
               .ThenInclude(a => a.BookCopy)
               .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
               .Include(s => s.Subscriptions)
               .Include(s => s.Rentals)
               .ThenInclude(r => r.RentalCopies)
               .SingleOrDefault(s => s.Id == rental.SubscriberId);

            var (errorMessage, maxAllowedCopies) = validateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();
            var currentCopies = _context.BookCopies.Where(c => currentCopiesIds.Contains(c.Id))
                .Include(c => c.Book)
                .ToList();

            var viewModel = new RentalFormViewModel()
            {
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };
            return View("Form", viewModel);

        }
        [HttpPost]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _context.Rentals
              .Include(r => r.RentalCopies)
              .SingleOrDefault(r => r.Id == model.id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers
               .Include(s => s.Subscriptions)
               .Include(s => s.Rentals)
               .ThenInclude(r => r.RentalCopies)
               .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = validateSubscriber(subscriber, model.id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalError, copies) = validdateCopies(model.SelectedCopies, subscriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalError))
                return View("NotAllowedRental", rentalError);


            rental.RentalCopies = copies!;
            rental.LastUpdatedById = User.GetUserId(); ;
            rental.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        public IActionResult Return(int id)
        {
            var rental = _context.Rentals
               .Include(r => r.RentalCopies)
               .ThenInclude(c => c.BookCopy)
               .ThenInclude(c => c!.Book)
               .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
               .Include(s => s.Subscriptions)
               .SingleOrDefault(s => s.Id == rental.SubscriberId);

            var viewModel = new ReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue)),
                SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(r => new ReturnCopuViewModel { Id = r.BookCopyId, isReturned = r.ExtendedOn.HasValue ? false : null }).ToList(),
                AllowExtend = !subscriber!.IsBlockListed
                && subscriber.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigrations.MaxRentalDuration)
                && rental.StartDate.AddDays((int)RentalsConfigrations.RentalDuration) >= DateTime.Today
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Return(ReturnFormViewModel model)
        {

            var rental = _context.Rentals
               .Include(r => r.RentalCopies)
               .ThenInclude(c => c.BookCopy)
               .ThenInclude(c => c!.Book)
               .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList());

            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);

            }

            var subscriber = _context.Subscribers
               .Include(s => s.Subscriptions)
               .SingleOrDefault(s => s.Id == rental.SubscriberId);

            if (model.SelectedCopies.Any(c => c.isReturned.HasValue && !c.isReturned.Value))
            {
                string errorMessage = string.Empty;

                if (subscriber!.IsBlockListed)
                    errorMessage = Errors.RentalNotAllowedBlacklisted;
                else if (subscriber.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigrations.MaxRentalDuration))
                    errorMessage = Errors.RentalNotAllowedInactive;
                else if (rental.StartDate.AddDays((int)RentalsConfigrations.RentalDuration) < DateTime.Today)
                    errorMessage = Errors.ExtendedNotAllowed;

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", errorMessage);
                    return View(model);
                }
            }

            var isUpdate = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.isReturned.HasValue) continue;
                var currentCopy = rental.RentalCopies.SingleOrDefault(r => r.BookCopyId == copy.Id);
                if (currentCopy is null) continue;

                if (copy.isReturned.HasValue && copy.isReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue) continue;
                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdate = true;
                }
                if (copy.isReturned.HasValue && !copy.isReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue) continue;
                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigrations.MaxRentalDuration);
                    isUpdate = true;
                }

            }

            if (isUpdate)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = User.GetUserId(); ;
                rental.PenaltyPaid = model.PenaltyPaid;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies
                .Include(b => b.Book)
                .FirstOrDefault(b => b.SerialNumber.ToString() == model.value && !b.IsDeleted && !b.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.isAvailableForRentel || !copy.Book!.isAvailableForRentel)
                return BadRequest(Errors.NotAvailableForRental);

            // check that copy is not in rental
            var copyIsRental = _context.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);
            if (copyIsRental)
                return BadRequest(Errors.CopyIsRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        }

        public IActionResult Details(int id)
        {

            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(a => a.BookCopy)
                .ThenInclude(a => a!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            _context.SaveChanges();

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult MarkAsDeleted(int id)
        {

            var rental = _context.Rentals.Find(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.GetUserId(); ;

            _context.SaveChanges();

            return Ok();
        }

        private (string errorMessage, int? maxAllowedCopies) validateSubscriber(Subscriber subscriber, int? RentalId = null)
        {
            if (subscriber.IsBlockListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);


            var currentRental = subscriber.Rentals
                .Where(r => r.Id != RentalId || RentalId == null)
                .SelectMany(r => r.RentalCopies)
                .Count(r => !r.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRental;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }
        private (string rentalError, ICollection<RentalCopy>? copies) validdateCopies(IEnumerable<int> SelectedSerial, int subscriberId, int? RentalId = null)
        {

            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => SelectedSerial.Contains(c.SerialNumber))
                .ToList();

            var currentSubscriberRental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId && (RentalId == null || r.Id != RentalId))
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {

                if (!copy.isAvailableForRentel || !copy.Book!.isAvailableForRentel)
                    return (rentalError: Errors.NotAvailableForRental, copies: null);

                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (RentalId == null || c.RentalId != RentalId)))
                    return (rentalError: Errors.CopyIsRental, copies: null);


                if (currentSubscriberRental.Any(bookId => bookId == copy.BookId))
                    return (rentalError: $"this subscriber already has a copy for {copy.Book.Title} book ", copies: null);


                copies.Add(new RentalCopy { BookCopyId = copy.Id });

            }

            return (rentalError: string.Empty, copies: copies);
        }


    }

}
