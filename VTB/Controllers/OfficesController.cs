using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VTB.DatabaseModels;
using VTB.Models.OfficeTransferModels;
using VTB.Services.Offices;

namespace VTB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficesController : ControllerBase
    {
        private readonly IOfficesService _officesService;
        private readonly VtbContext _vbContext;

        public OfficesController(IOfficesService officesService, VtbContext context)
        {
            _officesService = officesService;
            _vbContext = context;
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



        //[HttpGet("temp")]
        //public async Task<ActionResult> CreateRandom()
        //{
        //    var offices = await _vbContext.Offices.ToListAsync();
        //    var services = await _vbContext.Services.ToListAsync();
        //    foreach (var vbContextOffice in _vbContext.Offices)
        //    {
        //        HashSet<int> idSet = new HashSet<int>();
        //        for (int i = 0; i < 6; i++)
        //        {
        //            int id = Random.Shared.Next(1, 7);
        //            if (!idSet.Contains(id))
        //            {
        //                idSet.Add(id);
        //                vbContextOffice.Services.Add(services.First(x => x.Id == id));
        //            }
        //        }
        //    }
        //    await _vbContext.SaveChangesAsync();
        //    return Content("Ok всё!");
        //}
    }
}
