using FluentValidation;
using FluentValidation.AspNetCore;

namespace Bookify.Controllers
{
    [Authorize(Roles = AppRole.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IValidator<UserFormViewModel> _validator;
        private readonly IValidator<ResetPasswordFomViewModel> _validatorResetPassword;



        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder, IValidator<UserFormViewModel> validator, IValidator<ResetPasswordFomViewModel> validatorResetPassword)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _validator = validator;
            _validatorResetPassword = validatorResetPassword;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModel);
        }
        [AjaxOnly]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var ViewModel = new UserFormViewModel
            {
                Roles = await _roleManager.Roles
                            .Select(r => new SelectListItem
                            {
                                Text = r.Name,
                                Value = r.Name
                            })
                            .ToListAsync()
            };

            return PartialView("_Form", ViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            ApplicationUser user = new()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.UserName,
                CreatedById = User.GetUserId(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.SelectRoles);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var placeholders = new Dictionary<string, string>()
                {
                    {  "imageUrl","https://res.cloudinary.com/cloudomar/image/upload/v1720110498/Confirmed_attendance-rafiki_tm9g0r.png" },
                    {  "header", $"Hey {user.FullName},Thanks for joining us!" },
                    {  "body", "please confirm your email" },
                    {  "url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" },
                    {  "linkTitle",  "Active Account!"}
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplate.email, placeholders);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));
        }
        [AjaxOnly]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = _mapper.Map<UserFormViewModel>(user);
            viewModel.SelectRoles = await _userManager.GetRolesAsync(user);
            viewModel.Roles = await _roleManager.Roles
                            .Select(r => new SelectListItem
                            {
                                Text = r.Name,
                                Value = r.Name
                            })
                            .ToListAsync();

            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            var resultValidator = _validator.Validate(model);
            if (!resultValidator.IsValid)
                resultValidator.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);
            user.LastUpdatedById = User.GetUserId(); ;
            user.LastUpdatedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var RolesUpdate = !currentRoles.SequenceEqual(model.SelectRoles);
                if (RolesUpdate)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectRoles);
                }
                var viewModel = _mapper.Map<UserViewModel>(user);

                return PartialView("_UserRow", viewModel);
            }
            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswordFomViewModel { Id = id };

            return PartialView("_ResetForm", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordFomViewModel model)
        {
            var resultValidator = _validatorResetPassword.Validate(model);

            if (!ModelState.IsValid)
                return PartialView("_ResetForm", model);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedById = User.GetUserId(); ;
                user.LastUpdatedOn = DateTime.Now;

                await _userManager.UpdateAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);

                return PartialView("_UserRow", viewModel);
            }

            user.PasswordHash = currentPasswordHash;

            await _userManager.UpdateAsync(user);

            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = User.GetUserId(); ;
            user.LastUpdatedOn = DateTime.Now;

            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);


            return Ok(user.LastUpdatedOn.ToString());
        }
        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isAllowed = user is null || user.Id.Equals(model.Id);
            return Json(isAllowed);
        }
        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            bool isAllowed = user is null || user.Id.Equals(model.Id);
            return Json(isAllowed);
        }
        public async Task<IActionResult> UnlockUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
                return NotFound();

            var isLockout = await _userManager.IsLockedOutAsync(user); // lockout or Unlockout

            if (isLockout)
                await _userManager.SetLockoutEndDateAsync(user, null);// Remove lockout

            return Ok();
        }
    }
}
