using Bookify.Application.Common.Services.Books;
using Bookify.Application.Common.Services.Dashborad;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IBookServices, BookServices>();
            services.AddScoped<IDashboradServices, DashboradServices>();
            return services;
        }
    }
}
