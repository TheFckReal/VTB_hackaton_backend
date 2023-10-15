namespace VTB.Models.OSRMModels
{
    public class Route
    {
        public List<Leg> legs { get; set; }
        public string weight_name { get; set; }
        public double weight { get; set; }
        public double duration { get; set; }
        public decimal distance { get; set; }
    }
}
