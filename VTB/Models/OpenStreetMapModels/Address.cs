using System.Text.Json.Serialization;

namespace VTB.Models.OpenStreetMapModels
{
    public class Address
    {
        public string house_number { get; set; }
        public string road { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string state { get; set; }

        [JsonPropertyName("ISO3166-2-lvl4")]
        public string ISO31662lvl4 { get; set; }
        public string region { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }
}
