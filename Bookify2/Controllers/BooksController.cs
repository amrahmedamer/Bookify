using CloudinaryDotNet;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using Bookify.Application.Common.Services.Books;

namespace Bookify.web.Controllers
{
    [Authorize(Roles = AppRole.Archive)]
    public class BooksController : Controller
    {
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedExtentions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152;
        private readonly IImageService _imageService;
        private readonly IValidator<BookFormViewModel> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookServices _bookServices;



        public BooksController(
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment,
            IOptions<CloudinarySettings> cloudinary,
            IImageService imageService,
            IValidator<BookFormViewModel> validator,
            IUnitOfWork unitOfWork,
            IBookServices bookServices)
        {
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            Account account = new()
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.Apikey,
                ApiSecret = cloudinary.Value.Apisecret
            };

            _cloudinary = new Cloudinary(account);
            _imageService = imageService;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _bookServices = bookServices;
        }


        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult GetBooks()
        {
            var filteredDto = Request.Form.GetFilters();

            var (recordsTotal, book) = _bookServices.GetBooks(filteredDto);

            var mappedData = _mapper.ProjectTo<BookRowViewModel>(book).ToList();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, Data = mappedData };

            return Ok(jsonData);
        }
        public IActionResult Details(int id)
        {
            var book = _unitOfWork.Books
                .GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.BookCopies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            var viewModel = _mapper.ProjectTo<BookViewModel>(book).SingleOrDefault(b => b.Id == id);

            if (viewModel is null)
                return NotFound();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                var imageName = $"{Guid.NewGuid()}{extension}";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/Images/books/", thumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"/Images/books/{imageName}";
                model.ImageThumbnailUrl = $"/Images/books/thumb/{imageName}";

            }
            book.CreatedById = User.GetUserId();

            foreach (var category in model.SelectCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });


            _unitOfWork.Books.Add(book);
            _unitOfWork.Complete();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _unitOfWork.Books
                .GetQueryable()
                .Include(b => b.Categories)
                .SingleOrDefault(b => b.Id == id);

            if (book is null)
                return NotFound();

            var Model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(Model);
            viewModel.SelectCategories = book.Categories.Select(s => s.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _unitOfWork.Books.GetQueryable().Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            //string ImagePublicId = null;

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.ImageThumbnailUrl);

                    // await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/Images/books/", thumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"/Images/books/{imageName}";
                model.ImageThumbnailUrl = $"/Images/books/thumb/{imageName}";

            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;

            }

            book = _mapper.Map(model, book);
            book.LastUpdatedById = User.GetUserId();
            book.LastUpdatedOn = DateTime.Now;


            foreach (var category in model.SelectCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            if (!model.isAvailableForRentel)
                foreach (var copy in book.BookCopies)
                    copy.isAvailableForRentel = false;


            _unitOfWork.Complete();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _unitOfWork.Books.GetById(id);

            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = User.GetUserId();
            book.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            return Ok(book.LastUpdatedOn.ToString());
        }
        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;

            var author = _unitOfWork.Authors.GetQueryable().Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var category = _unitOfWork.Categories.GetQueryable().Where(a => !a.IsDeleted).OrderBy(a => a.CategoryName).ToList();

            viewModel.Author = _mapper.Map<IEnumerable<SelectListItem>>(author);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(category);

            return viewModel;
        }

        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _unitOfWork.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);
            return Json(isAllowed);
        }

        private string GetThumbnailUrl(string Url)
        {
            var separator = "image/upload/";
            var urlParts = Url.Split(separator);
            var url = $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";

            return url;
        }
    }
}
