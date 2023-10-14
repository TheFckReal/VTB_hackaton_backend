using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using VTB.Models.OfficeTransferModels;
using VTB.Models.OpenStreetMapModels;

namespace VTB.Services.User
{
    public class UserService : IUserService
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<Address?> GetAddressFromPointAsync(double lon, double lat)
        {
            string latStr = lat.ToString(CultureInfo.GetCultureInfo("en-US"));
            string lonStr = lon.ToString(CultureInfo.GetCultureInfo("en-US"));

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
            var requestResult = await httpClient.GetAsync($"https://nominatim.openstreetmap.org/reverse?lat={latStr}&lon={lonStr}&format=json");

            Address? address = null;
            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var reverseAddressComponents = await requestResult.Content.ReadFromJsonAsync<ReverseModel>();
                address = reverseAddressComponents?.address;
                return address;
            }

            return address;
        }

        public async Task<List<PointWithAddress>?> GetPointWithDataFromAddressAsync(string query)
        {
           var httpClient = _httpClientFactory.CreateClient();
           httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
           string message = $"q={query}&format=json";
           var requestResult = await httpClient.GetAsync("https://nominatim.openstreetmap.org/search?" + message);
           List<PointWithAddress>? addressList = null;
           if (requestResult.StatusCode == HttpStatusCode.OK)
           {
               var foundedOffices = await requestResult.Content.ReadFromJsonAsync<List<AddressFromPointModel>>();
               if (foundedOffices?.Count > 5)
               {
                   foundedOffices = foundedOffices.GetRange(0, 5);
               } else if (foundedOffices is null)
                   return null;

               addressList = new();
               foreach (var foundedOffice in foundedOffices)
               {
                   addressList.Add(new ()
                   {
                       DisplayName = RemoveExtraInfo(foundedOffice.display_name),
                       Point = new OfficesDTO.Point()
                       {
                           SRID = 4326,
                           Lon = double.Parse(foundedOffice.lon, CultureInfo.GetCultureInfo("en-US")),
                           Lat = double.Parse(foundedOffice.lat, CultureInfo.GetCultureInfo("en-US"))
                       }
                   });
               }
           }
           return addressList;

        }

        private string RemoveExtraInfo(string address)
        {
            int count = 0;
            for (int i = 0; i < address.Length; i++)
            {
                if (count == 4)
                {
                    address = address.Remove(i - 1);
                    break;
                }
                if (address[i] == ',')
                {
                    count++;
                }
            }
            return address;
        }

        public record PointWithAddress
        {
            public OfficesDTO.Point Point { get; set; }
            public string DisplayName { get; set; } = null!;
        }
    }
}
