using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VTB.DatabaseModels;
using VTB.Models;
using VTB.Models.OfficeTransferModels;
using VTB.Services.Offices;

namespace VTB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficesController : ControllerBase
    {
        private readonly IOfficesService _officesService;

        public OfficesController(IOfficesService officesService)
        {
            _officesService = officesService;
        }

        /// <summary>
        /// Return list of all offices info
        /// </summary>
        /// <returns>List of all offices info</returns>
        [HttpGet("all")]
        public async Task<ActionResult<List<OfficesDTO>>> GetAllOfficesAsync()
        {
            List<OfficesDTO> result = await _officesService.GetListOfAllOfficesAsync();
            return result;
        }

        /// <summary>
        /// Allows to get offices within radius
        /// </summary>
        /// <param name="ratio">Coefficient of scale</param>
        /// <param name="x">Lon component of point</param>
        /// <param name="y">Lat component of point</param>
        /// <returns></returns>
        [HttpGet("{ratio}")]
        public async Task<ActionResult<List<OfficesDTO>>> GetOfficesInRadius(int ratio, double x, double y)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var foundedOffices = await _officesService.GetListOfNearestOfficesAsync(ratio, new OfficesDTO.Point() { SRID = 4326, Lon = x, Lat = y });

            return foundedOffices;
        }

        /// <summary>
        /// Allows to get optimum department in the calculation of workload
        /// </summary>
        /// <param name="servicesIds">Array of client`s required services</param>
        /// <param name="point">Point of user geolocation</param>
        /// <returns>Info about optimal department</returns>
        [HttpGet("optimum")]
        public async Task<ActionResult<OfficesDTO?>> FindOptimumOffice([Required, FromQuery] List<int> servicesIds, [Required, FromQuery] OfficesDTO.Point point)
        {
            var optimumOffice = await _officesService.FindOptimumOfficeAsync(servicesIds, point);
            if (optimumOffice is null)
                return NotFound();
            else
                return optimumOffice;
        }

        /// <summary>
        /// Allows to get coordinate array of route from begin point to end point
        /// </summary>
        /// <param name="fLon">Longitude of point from</param>
        /// <param name="fLat">Latitude of point from</param>
        /// <param name="tLon">Longitude of point to</param>
        /// <param name="tLat">Longitude of point to</param>
        /// <param name="profile">Type of movement in format {car/foot/bike}</param>
        /// <returns>Coordinates of route from one point to another with time</returns>
        [HttpGet("route")]
        public async Task<ActionResult<DTORoute>> GetRouteFromPointToOffice(double fLon,
            double fLat, double tLon, double tLat, string profile)
        {
            var resultCoordinates = await _officesService.GetRoutePoints(fLon, fLat, tLon, tLat, profile);
            if (resultCoordinates.Item1 is null)
                return NotFound();
            else
            {
                var result = new DTORoute()
                {
                    Time = resultCoordinates.time ?? 0,
                    Coordinates = new()
                };
                foreach (var coordinate in resultCoordinates.Item1)
                {
                    result.Coordinates.Add(new()
                    {
                        Lat = coordinate.lat,
                        Lon = coordinate.lon,
                    });
                }
                return result;
            }
        }

        public record DTORoute
        {
            public List<Coordinate> Coordinates { get; set; }
            public double Time { get; set; }


            public record Coordinate
            {
                public double Lon { get; set; }
                public double Lat { get; set; }
            }

        }



    }
}
