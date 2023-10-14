using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using VTB.DatabaseModels;
using VTB.Models.OfficeTransferModels;
using Microsoft.EntityFrameworkCore.Query;

namespace VTB.Services.Offices
{
    public class OfficeService : IOfficesService
    {
        private readonly VtbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public OfficeService(VtbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        private List<OfficesDTO> FromOfficeToDto(List<DatabaseModels.Office> foundedOffices)
        {
            List<OfficesDTO> officesDTO = new List<OfficesDTO>();
            foreach (var office in foundedOffices)
            {
                var newOffice = new OfficesDTO()
                {
                    Id = office.Id,
                    Name = office.Name,
                    Address = office.Address,
                    OfficePoint = new()
                    {
                        SRID = office.Point.SRID,
                        Lon = office.Point.Coordinate.X,
                        Lat = office.Point.Coordinate.Y
                    },
                    HasRamp = office.HasRamp,
                    Status = office.Status
                };

                newOffice.OpenHoursData = new List<OfficesDTO.OpenHours>();
                if (office.OpenHours is not null)
                {
                    foreach (var officeOpenHour in office.OpenHours)
                    {
                        newOffice.OpenHoursData.Add(new()
                        {
                            Days = officeOpenHour.Days,
                            Hours = officeOpenHour.Hours
                        });
                    }
                }

                newOffice.OpenHoursIndividualData = new List<OfficesDTO.OpenHourIndividual>();
                if (office.OpenHoursIndividuals is not null)
                {
                    foreach (var openHoursIndividual in office.OpenHoursIndividuals)
                    {
                        newOffice.OpenHoursIndividualData.Add(new()
                        {
                            Days = openHoursIndividual.Days,
                            Hours = openHoursIndividual.Hours
                        });
                    }
                }

                officesDTO.Add(newOffice);
            }

            return officesDTO;
        }

        public async Task<List<OfficesDTO>> GetListOfAllOfficesAsync()
        {
            var foundedOffices = await _context.Offices.Include(x => x.OpenHours).Include(x => x.OpenHoursIndividuals)
                .Include(x => x.Services).ToListAsync();
            var officesDTO = FromOfficeToDto(foundedOffices);

            return officesDTO;
        }

        public async Task<List<OfficesDTO>> GetListOfNearestOfficesAsync(int ratio, OfficesDTO.Point point)
        {
            point.SRID = 4326;
            const int constantDistance = 100;
            double radius = 1.0 / ratio * constantDistance;

            var officesList = await _context.Offices.Include(x => x.OpenHours).Include(x => x.OpenHoursIndividuals)
                .Include(x => x.Services)
                .Where(x => x.Point.IsWithinDistance(new Point(point.Lon, point.Lat), radius)).ToListAsync();

            var officesDtoList = FromOfficeToDto(officesList);

            return officesDtoList;
        }

        public async Task<Office> FindOptimumOfficeAsync(List<int> servicesIds, OfficesDTO.Point point)
        {
            var offices = await _context.Offices.IntersectBy(servicesIds, x => x.Id).ToListAsync();
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.GetAsync();
        }
    }
}
