using FluentValidation.AspNetCore;
namespace Bookify.web.Controllers
{
    [Authorize(Roles = AppRole.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryFormViewModel> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController( IMapper mapper, IValidator<CategoryFormViewModel> validator, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categories = _unitOfWork.Categories.GetAll(false);
            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            return View(viewModel);
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {

            return PartialView("_Form");
        }
        [HttpPost]
        public IActionResult Create(CategoryFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var category = _mapper.Map<Category>(model);
            category.CreatedById = User.GetUserId();

            _unitOfWork.Categories.Add(category);
            _unitOfWork.Complete();

            var viewmodel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewmodel);
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);

            if (category is null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryFormViewModel>(category);

            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var category = _unitOfWork.Categories.GetById(model.CategoryId);

            if (category is null)
                return NotFound();

            category = _mapper.Map(model, category);
            category.LastUpdatedById = User.GetUserId();
            category.LastUpdatedOn = DateTime.Now;

             _unitOfWork.Categories.Update(category);
            _unitOfWork.Complete();

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewModel);
        }
        public IActionResult Delete(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);
            _unitOfWork.Categories.Remove(category!);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedById = User.GetUserId();
            category.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            return Ok(category.LastUpdatedOn.ToString());
        }
        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            var category = _unitOfWork.Categories.SingleOrDefault(c => c.CategoryName == model.CategoryName);
            var isAllowed = category is null || category.CategoryId.Equals(model.CategoryId);
            return Json(isAllowed);
        }
    }
}
