using VTB.Models.OfficeTransferModels;

namespace VTB.Services.Offices
{
    public interface IOfficesService
    {
        public Task<List<OfficesDTO>> GetListOfAllOfficesAsync();
        public Task<List<OfficesDTO>> GetListOfNearestOfficesAsync(int ratio, OfficesDTO.Point point);
    }
}
