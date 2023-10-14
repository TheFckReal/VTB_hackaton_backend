namespace VTB.Models.OSRMModels
{
    public class Leg
    {
        public List<object> steps { get; set; }
        public string summary { get; set; }
        public double weight { get; set; }
        public double duration { get; set; }
        public int distance { get; set; }
    }
}
