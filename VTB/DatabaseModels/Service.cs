using System;
using System.Collections.Generic;

namespace VTB.DatabaseModels;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Office> Offices { get; set; } = new List<Office>();
}
