using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using VTB.DatabaseModels;
using VTB.Models.OfficeTransferModels;
using Microsoft.EntityFrameworkCore.Query;
using VTB.Models.OSRMModels;

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
                    Status = office.Status,
                    Queue = office.CurrentQueue.GetValueOrDefault(),
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

                newOffice.ServicesData = new List<OfficesDTO.Services>();
                if (office.Services is not null)
                {
                    foreach (Service service in office.Services)
                    {
                        newOffice.ServicesData.Add(new()
                        {
                            Id = service.Id,
                            Name = service.Name
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
            const double constantDistance = 0.005;
            double radius = (18 - ratio) * constantDistance;

            var officesList = await _context.Offices.Include(x => x.OpenHours).Include(x => x.OpenHoursIndividuals)
                .Include(x => x.Services)
                .Where(x => x.Point.IsWithinDistance(new Point(point.Lon, point.Lat) { SRID = 4326 }, radius)).ToListAsync();

            var officesDtoList = FromOfficeToDto(officesList);

            return officesDtoList;
        }

        /// <summary>
        /// Optimum office calculates as Y = a * q + t * d where a - average waiting time in queue, q - count of people in queue, t - time of path, d - dynamic of people movement
        /// </summary>
        /// <param name="servicesIds"></param>
        /// <param name="point"></param>
        /// <returns>Founded optimum office or null</returns>
        public async Task<OfficesDTO?> FindOptimumOfficeAsync(List<int> servicesIds, OfficesDTO.Point point)
        {
            var foundedServices = _context.Services.Where(x => servicesIds.Contains(x.Id));

            var offices = await _context.Services.Where(service => foundedServices.Contains(service)).SelectMany(s => s.Offices).Include(x => x.Services)
                .OrderBy(x => EF.Functions.DistanceKnn(x.Point, new Point(point.Lon, point.Lat) { SRID = 4326 })).Take(20).ToListAsync();


            (double, Office?) optimumValueOffice = (double.MaxValue, null);
            var httpClient = _httpClientFactory.CreateClient();
            foreach (Office office in offices)
            {
                string userLat = point.Lat.ToString(CultureInfo.GetCultureInfo("en-US"));
                string userLon = point.Lon.ToString(CultureInfo.GetCultureInfo("en-US"));
                string officeLat = office.Point.Coordinate.Y.ToString(CultureInfo.GetCultureInfo("en-US"));
                string officeLon = office.Point.Coordinate.X.ToString(CultureInfo.GetCultureInfo("en-US"));
                var requestResult = await httpClient.GetAsync(
                    $"http://router.project-osrm.org/route/v1/driving/{userLon},{userLat};{officeLon},{officeLat}?geometries=polyline");
                if (requestResult.StatusCode == HttpStatusCode.OK)
                {
                    var root = await requestResult.Content.ReadFromJsonAsync<Root>();
                    var route = root.routes.First();

                    double dQueue = office.CurrentQueue.GetValueOrDefault(0);
                    double duration = route.duration;
                    double dDynamicQueueChange = office.DynamicQueueChange.GetValueOrDefault(0);
                    OptimumVariablesNormalize(ref dQueue, ref dDynamicQueueChange, ref duration);
                    // Time in minutes, distance in km
                    double? y = dQueue * office.AverageWaiting.Value + duration * dDynamicQueueChange;

                    if (y is not null && y < optimumValueOffice.Item1)
                    {
                        optimumValueOffice.Item1 = y.Value;
                        optimumValueOffice.Item2 = office;
                    }
                }
            }

            OfficesDTO? resultOfficesDto = null;
            if (optimumValueOffice.Item2 is not null)
            {
                resultOfficesDto = FromOfficeToDto(new() { optimumValueOffice.Item2 }).First();
            }
            return resultOfficesDto;
        }

        /// <summary>
        /// Normalizes variables by minimax method
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="queueChanging"></param>
        /// <param name="duration"></param>
        private void OptimumVariablesNormalize(ref double queue, ref double queueChanging, ref double duration)
        {
            double max, min;
            max = Math.Max(Math.Max(queue, queueChanging), duration);
            min = Math.Min(Math.Min(queue, queueChanging), duration);
            int a = -1, b = 1;
            queue = a + (queue - min) / (max - min) * (b - a);
            queueChanging = a + (queueChanging - min) / (max - min) * (b - a);
            duration = a + (duration - min) / (max - min) * (b - a);
        }

        public async Task<(List<(double lon, double lat)>?, double? time)> GetRoutePoints(double fLon,
            double fLat, double tLon, double tLat, string profile)
        {
            string fLonStr = fLon.ToString(CultureInfo.GetCultureInfo("en-US"));
            string fLatStr = fLat.ToString(CultureInfo.GetCultureInfo("en-US"));
            string tLonStr = tLon.ToString(CultureInfo.GetCultureInfo("en-US"));
            string tLatStr = tLat.ToString(CultureInfo.GetCultureInfo("en-US"));

            var httpClient = _httpClientFactory.CreateClient();
            double timeCoef = 1;
            timeCoef = profile switch
            {
                "car" => 1,
                "bike" => 0.5,
                "foot" => 0.2,
                _ => 1
            };
            var requestResult = await httpClient.GetAsync(
                $"http://router.project-osrm.org/route/v1/{profile}/{fLonStr},{fLatStr};{tLonStr},{tLatStr}?geometries=geojson");
            List<(double, double)>? coordinates = null;
            double? time = null;
            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var routes = await requestResult.Content.ReadFromJsonAsync<Models.OSRMModels.OSRMRouteModels.Root>();
                var currentRoute = routes?.routes.FirstOrDefault();
                if (currentRoute is not null)
                {
                    coordinates = new List<(double, double)>();
                    foreach (var geometryCoordinate in currentRoute.geometry.coordinates)
                    {
                        coordinates.Add(new()
                        {
                            Item1 = geometryCoordinate[0],
                            Item2 = geometryCoordinate[1],
                        });
                    }

                    time = currentRoute.duration * timeCoef;
                }
            }

            return (coordinates, time);
        }
    }
}
