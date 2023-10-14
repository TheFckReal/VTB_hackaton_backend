using VTB.Services.Offices;
using VTB.Services.User;

namespace VTB.Extensions
{
    public static class OfficesServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IOfficesService, OfficeService>();
            serviceCollection.AddScoped<IUserService, UserService>();
            return serviceCollection;
        }
    }
}
