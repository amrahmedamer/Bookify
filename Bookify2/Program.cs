using Bookify.Application;
using Bookify.Filter;
using Bookify.Infrastructure;
using Bookify.Middlewares;
using Bookify.Seeds;
using Bookify.Tasks;
using Hangfire.Dashboard;
using Serilog;
using Serilog.Context;

namespace Bookify
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var cultures = new[] { "en-US", "ar-EG" };

            // Add services to the container.
            builder.Services
                .AddLocalizationConfigrations(cultures)
                .AddInfrastructureService(builder.Configuration)
                .AddApplicationService()
                .AddWebService(builder);
            //add serilog
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog();

            builder.Services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            var app = builder.Build();

            var localizationOptions = new RequestLocalizationOptions()
                .AddSupportedCultures(cultures)
                .AddSupportedUICultures(cultures);
            app.UseRequestLocalization(localizationOptions);
            app.UseRequestCulture();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //   app.UseExceptionHandler("/Home/Error");


            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "Deny");
                await next();
            });
            //   app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            var ScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using var Scope = ScopeFactory.CreateScope();
            var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await DefaultRole.SeedRolesAsync(roleManager);
            await DefaultUser.SeedAdminUserAsync(userManager);

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "Bookify Dashboard",
                // IsReadOnlyFunc = (DashboardContext context) => true,
                Authorization = new IDashboardAuthorizationFilter[]
                {
                    new HangfireAuthorizationFilter("AdminOnly")
                }
            });
            var dbcontext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var webHostEnvironment = Scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var whatsAppClient = Scope.ServiceProvider.GetRequiredService<IWhatsAppClient>();
            var emailSender = Scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var emailBodyBuilder = Scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();


            var hangfireTasks = new HangfireTasks(dbcontext, webHostEnvironment, whatsAppClient, emailSender, emailBodyBuilder);

            //RecurringJob.AddOrUpdate(() => hangfireTasks.PrepareExpirationAlert(), "* * * * *");

            app.Use(async (context, next) =>
            {
                LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.Name)?.Value);
                await next();
            });
            app.UseSerilogRequestLogging();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}