namespace Bookify.Controllers
{
    [Authorize(Roles = AppRole.Reception)]
    public class SubscribersController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        public SubscribersController(IApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IDataProtectionProvider dataProtector, IMapper mapper, IImageService imageService, IWhatsAppClient whatsAppClient, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _imageService = imageService;
            _whatsAppClient = whatsAppClient;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }
        public IActionResult Index()
        {

            return View();
        }
        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = _context.Subscribers
                .Include(a => a.Governorate)
                .Include(a => a.Area)
                .Include(a => a.Subscriptions)
                .Include(a => a.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(a => a.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            ViewBag.SubscriberId = subscriberId;

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers.SingleOrDefault(a => a.Email == model.value
                || a.MobileNumber == model.value
                || a.NationalId == model.value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }
        [HttpPost]
        public IActionResult RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subscribers.Include(x => x.Subscriptions).SingleOrDefault(x => x.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlockListed)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.Last();
            var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

            Subscription newSubscription = new()
            {
                CreatedById = User.GetUserId(),
                CreatedOn = DateTime.Today,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)

            };
            subscriber.Subscriptions.Add(newSubscription);
            _context.SaveChanges();

            //TODO:send email message and whatsapp
            var placeholders = new Dictionary<string, string>()
            {
                {  "imageUrl","https://res.cloudinary.com/cloudomar/image/upload/c_thumb,w_200,g_face/v1721237889/Appreciation-bro_hkho7l.png" },
                {  "header", $"Hey {subscriber.FirstName},Thanks for renewed us!" },
                {  "body", $"Hello {subscriber.FirstName}, your subscription has been renewed through {startDate.AddYears(1)}🥳🥳🥳 "}

            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplate.notification, placeholders);

            //BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "Confirm your email", body));
            BackgroundJob.Schedule(() => _emailSender.SendEmailAsync(subscriber.Email, "Confirm your email", body), TimeSpan.FromMinutes(1));


            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type="body",
                        Parameters=new List<object>()
                        {
                            new WhatsAppTextParameter{Text=subscriber.FirstName},
                            new WhatsAppTextParameter{Text=startDate.AddYears(1).ToString("d MMM,yyyy")}
                        }
                    }
                };
                var number = _webHostEnvironment.IsDevelopment() ? "201029409898" : subscriber.MobileNumber;

                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(number, WhatsAppLanguageCode.English_US, WhatsAppTemplates.SubscriptionRenew, components));
            }

            var viewModel = _mapper.Map<subscriptionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", viewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);
            var folderPath = "/Images/Subscriper/";
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, folderPath, thumbnail: true);
            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            subscriber.ImagerUrl = $"{folderPath}{imageName}";
            subscriber.ImagerThumbnailUrl = $"{folderPath}thumb/{imageName}";
            subscriber.CreatedById = User.GetUserId(); ;

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
            };

            subscriber.Subscriptions.Add(subscription);

            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();

            var placeholders = new Dictionary<string, string>()
            {
                {  "imageUrl","https://res.cloudinary.com/cloudomar/image/upload/c_scale,h_100,w_250/v1720110498/Confirmed_attendance-rafiki_tm9g0r.png" },
                {  "header", $"Hey {model.FirstName},Thanks for joining us!" },
                {  "body", "Welcome Omar ,Thanks for joining Bookify 🤩" },

            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplate.notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(model.Email, "Confirm your email", body));

            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type="body",
                        Parameters=new List<object>()
                        {
                            new WhatsAppTextParameter{Text=model.FirstName}
                        }
                    }
                };
                var number = _webHostEnvironment.IsDevelopment() ? "01029409898" : model.MobileNumber;
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage($"2{number}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.WelcomeMessage, components));
            }
            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }
        [HttpGet]
        public IActionResult Edit(string Id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(Id));

            var subscriber = _context.Subscribers.Find(subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel.Key = Id;

            return View("Form", PopulateViewModel(viewModel));
        }
        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriperId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriper = _context.Subscribers.Find(subscriperId);

            if (subscriper is null)
                return NotFound();

            if (model.Image is not null)
            {

                if (!string.IsNullOrEmpty(subscriper.ImagerUrl))
                {
                    _imageService.Delete(subscriper.ImagerUrl, subscriper.ImagerThumbnailUrl);
                }

                var folderPath = "/Images/Subscriper/";
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, folderPath, thumbnail: true);
                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
                model.ImagerUrl = $"{folderPath}{imageName}";
                model.ImagerThumbnailUrl = $"{folderPath}thumb/{imageName}";
            }
            else if (!string.IsNullOrEmpty(subscriper.ImagerUrl))
            {
                model.ImagerUrl = subscriper.ImagerUrl;
                model.ImagerThumbnailUrl = subscriper.ImagerThumbnailUrl;
            }

            subscriper = _mapper.Map(model, subscriper);
            subscriper.LastUpdatedById = User.GetUserId(); ;
            subscriper.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }

        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var selectGovernorate = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            viewModel.Governorate = _mapper.Map<IEnumerable<SelectListItem>>(selectGovernorate);

            if (model?.GovernorateId > 0)
            {
                var selectArea = _context.Areas.Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();
                viewModel.Area = _mapper.Map<IEnumerable<SelectListItem>>(selectArea);
            }

            return viewModel;
        }
        [AjaxOnly]
        public IActionResult GetArea(int governratesId)
        {
            var selecArea = _context.Areas
                        .Where(g => g.GovernorateId == governratesId && !g.IsDeleted)
                        .OrderBy(a => a.Name)
                        .ToList();

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(selecArea));
        }
        public IActionResult AllowItemEmail(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriper = _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);
            var isAllow = subscriper is null || subscriper.Id.Equals(id);
            return Json(isAllow);
        }
        public IActionResult AllowItemNationalId(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriper = _context.Subscribers.SingleOrDefault(s => s.NationalId == model.NationalId);
            var isAllow = subscriper is null || subscriper.Id.Equals(id);
            return Json(isAllow);
        }
        public IActionResult AllowItemMobileNumber(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
                id = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriper = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);
            var isAllow = subscriper is null || subscriper.Id.Equals(id);
            return Json(isAllow);
        }
    }
}
