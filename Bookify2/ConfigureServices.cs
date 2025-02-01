using Bookify.Application.Common.Interfaces.Repositories;
using Bookify.Core.Mapper;
using Bookify.Helpers;
using Bookify.Infrastructure.Persistence.Repositories;
using Bookify.Localization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Serilog;
using System.Globalization;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;

namespace Bookify
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebService(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            services.AddExpressiveAnnotations();
            services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
            services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
             {
                 options.SignIn.RequireConfirmedAccount = true;
             })
                 .AddEntityFrameworkStores<ApplicationDbContext>()
                 .AddDefaultUI()
                 .AddDefaultTokenProviders();


            services.AddDataProtection().SetApplicationName(nameof(Bookify));
            services.AddSingleton<IHashids>(h => new Hashids("S#cUr3S@ltV@lu3!1234", minHashLength: 8));

            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);


            services.Configure<IdentityOptions>(options =>
             {
                 options.Password.RequiredLength = 8;
                 options.User.RequireUniqueEmail = false;

                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                 options.Lockout.MaxFailedAccessAttempts = 3;

             });

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddScoped<IAuthorReopsitory, AuthorReopsitory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddWhatsAppApiClient(builder.Configuration);

            services.AddControllersWithViews();
            services.AddHangfire(a => a.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            services.Configure<AuthorizationOptions>(options =>
             options.AddPolicy("AdminOnly", policy =>
             {
                 policy.RequireAuthenticatedUser();
                 policy.RequireRole(AppRole.Admin);
             }));

            // register ViewToHTML service
            services.AddViewToHTML();
            //register fluentApi 
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            //add serilog
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog();

            services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            return services;
        }

        public static IServiceCollection AddLocalizationConfigrations(this IServiceCollection services, string[] cultures)
        {
            services.AddLocalization();
            services.AddDistributedMemoryCache();
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

            services.AddMvc()
                 .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                 .AddDataAnnotationsLocalization(options =>
                 {
                     options.DataAnnotationLocalizerProvider = (type, factory) =>
                         factory.Create(typeof(JsonStringLocalizerFactory));
                 });

            services.Configure<RequestLocalizationOptions>(options =>
             {
                 var supportedCultures = cultures.Select(c=>new CultureInfo(c)).ToArray();

                 options.SupportedCultures = supportedCultures;
                 options.SupportedUICultures = supportedCultures;
             });
            return services;
        }
    }
}
 