﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookify.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [Display(Name = "Email/Username")]
            //[EmailAddress]
            public string Username { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var userName = Input.Username.ToUpper();
                var user = await _userManager.Users.SingleOrDefaultAsync(u => (u.NormalizedUserName == userName || u.NormalizedEmail == userName) && !u.IsDeleted);

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
                var placeholders = new Dictionary<string, string>()
                {
                    {  "imageUrl","https://res.cloudinary.com/cloudomar/image/upload/w_1000,c_fill,ar_1:1,g_auto,r_max,bo_5px_solid_red,b_rgb:262c35/v1720262352/Reset_password-rafiki_npw9yd.png" },
                    {  "header", $"Hey {user.FullName}" },
                    {  "body", "please click the below button to reset you password" },
                    {  "url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" },
                    {  "linkTitle",  "Active Account!"}
                };

                var body = _emailBodyBuilder.GetEmailBody("email", placeholders);

                await _emailSender.SendEmailAsync(user.Email, "Reset Password", body);


                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
