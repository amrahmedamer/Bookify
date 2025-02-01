namespace Bookify.Seeds
{
    public static class DefaultUser
    {

        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email = "admin@bookify.com",
                FullName = "Admin",
                EmailConfirmed = true,

            };

            var Users = await userManager.FindByEmailAsync(admin.Email);
            if (Users is null)
            {
                await userManager.CreateAsync(admin, "P@ssword123");
                await userManager.AddToRoleAsync(admin, AppRole.Admin);
            }
            //if (!result.Succeeded)
            //{
            //    foreach (var error in result.Errors)
            //    {
            //        // تسجيل أو معالجة الخطأ
            //        Console.WriteLine($"Error: {error.Code}, {error.Description}");
            //    }
            //}

        }
    }
}
