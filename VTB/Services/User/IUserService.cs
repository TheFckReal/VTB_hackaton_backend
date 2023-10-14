using VTB.Models.OfficeTransferModels;
using VTB.Models.OpenStreetMapModels;
using static VTB.Services.User.UserService;

namespace VTB.Services.User
{
    public interface IUserService
    {
        public Task<Address?> GetAddressFromPointAsync(double lon, double lat);
        public Task<List<PointWithAddress>?> GetPointWithDataFromAddressAsync(string query);
    }
}
