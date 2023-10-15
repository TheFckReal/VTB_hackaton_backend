using VTB.DatabaseModels;
using VTB.Models.OfficeTransferModels;

namespace VTB.Services.Offices
{
    public interface IOfficesService
    {
        public Task<List<OfficesDTO>> GetListOfAllOfficesAsync();
        public Task<List<OfficesDTO>> GetListOfNearestOfficesAsync(int ratio, OfficesDTO.Point point);

        public Task<OfficesDTO?> FindOptimumOfficeAsync(List<int> servicesIds, OfficesDTO.Point point);

        public Task<(List<(double lon, double lat)>?, double? time)> GetRoutePoints(double fLon,
            double fLat, double tLon, double tLat, string profile);
    }
}
