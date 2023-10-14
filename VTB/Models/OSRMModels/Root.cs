namespace VTB.Models.OSRMModels
{
    public class Root
    {
        public string code { get; set; }
        public List<Route> routes { get; set; }
        public List<Waypoint> waypoints { get; set; }
    }
}
