namespace VTB.DatabaseModels;

public partial class OpenHoursIndividual
{
    public int Id { get; set; }

    public string Days { get; set; } = null!;

    public string? Hours { get; set; }

    public int? OfficeId { get; set; }

    public virtual Office? Office { get; set; }
}
