using Microsoft.AspNetCore.Mvc;
using VTB.Models.OfficeTransferModels;
using VTB.Models.OpenStreetMapModels;
using VTB.Services.User;

namespace VTB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Allows to get address by point
        /// </summary>
        /// <param name="lon">Lon of point</param>
        /// <param name="lat">Lat of point</param>
        /// <returns>Detailed address information</returns>
        [HttpGet]
        public async Task<ActionResult<Address>> GetUserAddress(double lon, double lat)
        {
            Address? resultAddress = await _userService.GetAddressFromPointAsync(lon, lat);
            if (resultAddress is null)
                return NotFound();
            return Ok(resultAddress);
        }

        /// <summary>
        /// Allows to get latitude, longitude and full address name from address query
        /// </summary>
        /// <param name="query">Address query in free form</param>
        /// <returns>List (max objects:5) of points with detailed name</returns>
        [HttpGet("address/{query}")]
        public async Task<ActionResult<List<UserService.PointWithAddress>>> GetPointFromAddress(string query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var resultPoint = await _userService.GetPointWithDataFromAddressAsync(query);
            if (resultPoint is null)
                return NotFound();
            else
                return Ok(resultPoint);
        }
    }
}
