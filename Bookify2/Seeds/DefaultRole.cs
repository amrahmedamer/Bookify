namespace Bookify.Seeds
{
    public static class DefaultRole
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(AppRole.Admin));
                await roleManager.CreateAsync(new IdentityRole(AppRole.Archive));
                await roleManager.CreateAsync(new IdentityRole(AppRole.Reception));
            }

        }
    }
}
