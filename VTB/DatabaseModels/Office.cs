using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace VTB.DatabaseModels;

public partial class Office
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public Geometry Point { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool? HasRamp { get; set; }

    public int? CurrentQueue { get; set; }

    public double? AverageWaiting { get; set; }

    public double? DynamicQueueChange { get; set; }

    public virtual ICollection<OpenHour> OpenHours { get; set; } = new List<OpenHour>();

    public virtual ICollection<OpenHoursIndividual> OpenHoursIndividuals { get; set; } = new List<OpenHoursIndividual>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
