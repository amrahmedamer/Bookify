namespace Bookify.web.Controllers
{
    [Authorize(Roles = AppRole.Archive)]
    public class AuthorsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<AuthorFormViewModel> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public AuthorsController(IApplicationDbContext context, IMapper mapper, IValidator<AuthorFormViewModel> validator,  IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var authors = _unitOfWork.Authors.GetAll();
            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);

            return View(viewModel);

        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        public IActionResult Create(AuthorFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);

            if (!resultValidator.IsValid)
                return BadRequest();


            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.GetUserId();

            _unitOfWork.Authors.Add(author);
            _unitOfWork.Complete();

         

             var viewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", viewModel);
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _unitOfWork.Authors.GetById(id);

            if (author is null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorFormViewModel>(author);

            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);

            if (!resultValidator.IsValid)
                return BadRequest();

            var authorFromDB = _unitOfWork.Authors.GetById(model.id);

            if (authorFromDB is null)
                return NotFound();

            var update = _mapper.Map(model, authorFromDB);
            update.CreatedById = User.GetUserId();
            update.CreatedOn = DateTime.Now;

             _unitOfWork.Authors.Update(update);
            _unitOfWork.Complete();

            var viewModel = _mapper.Map<AuthorViewModel>(update);

            return PartialView("_AuthorRow", viewModel);
        }

        [HttpPost]

        public IActionResult ToggleStatus(int id)
        {
            var author = _unitOfWork.Authors.GetById(id);

            if (author is null)
                return NotFound();

            author.LastUpdatedById = User.GetUserId();
            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            return Ok(author.LastUpdatedOn.ToString());
        }
        public IActionResult AllowItem(AuthorFormViewModel model)
        {
            var author = _unitOfWork.Authors.SingleOrDefault(c => c.Name == model.Name);
            var isAllowed = author is null || author.id.Equals(model.id);
            return Json(isAllowed);
        }
    }
}
