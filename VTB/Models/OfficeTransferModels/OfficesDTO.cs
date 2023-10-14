namespace VTB.Models.OfficeTransferModels
{
    public record OfficesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public Point OfficePoint { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool? HasRamp { get; set; }
        public List<OpenHours> OpenHoursData { get; set; } = null!;
        public List<OpenHourIndividual> OpenHoursIndividualData { get; set; } = null!;
        public List<Services>? ServicesData { get; set; }

        public record Point
        {
            public double Lon { get; set; }
            public double Lat { get; set; }
            public int SRID { get; set; }
        }

        public record OpenHours
        {
            public string Days { get; set; } = null!;
            public string? Hours { get; set; }
        }

        public record OpenHourIndividual
        {
            public string Days { get; set; } = null!;
            public string? Hours { get; set; }
        }

        public record Services
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
