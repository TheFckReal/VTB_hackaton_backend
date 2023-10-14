using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace VTB.DatabaseModels;

public partial class VtbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public VtbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<OpenHour> OpenHours { get; set; }

    public virtual DbSet<OpenHoursIndividual> OpenHoursIndividuals { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(_configuration["DefaultConnectionString"], x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("topology", "postgis_topology");

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("offices_pk");

            entity.ToTable("offices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(400)
                .HasColumnName("address");
            entity.Property(e => e.HasRamp).HasColumnName("hasRamp");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Point).HasColumnName("point");
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasColumnName("status");

            entity.HasMany(d => d.Services).WithMany(p => p.Offices)
                .UsingEntity<Dictionary<string, object>>(
                    "ServicesOffice",
                    r => r.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("servicesOffices_services_id_fk"),
                    l => l.HasOne<Office>().WithMany()
                        .HasForeignKey("OfficeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("servicesOffices_offices_id_fk"),
                    j =>
                    {
                        j.HasKey("OfficeId", "ServiceId").HasName("servicesOffices_pk");
                        j.ToTable("servicesOffices");
                        j.IndexerProperty<int>("OfficeId")
                            .ValueGeneratedOnAdd()
                            .HasColumnName("office_id");
                        j.IndexerProperty<int>("ServiceId")
                            .ValueGeneratedOnAdd()
                            .HasColumnName("service_id");
                    });
        });

        modelBuilder.Entity<OpenHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("openHours_pk");

            entity.ToTable("openHours");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Days)
                .HasMaxLength(50)
                .HasColumnName("days");
            entity.Property(e => e.Hours)
                .HasMaxLength(50)
                .HasColumnName("hours");
            entity.Property(e => e.OfficeId).HasColumnName("office_id");

            entity.HasOne(d => d.Office).WithMany(p => p.OpenHours)
                .HasForeignKey(d => d.OfficeId)
                .HasConstraintName("openHours_offices_id_fk");
        });

        modelBuilder.Entity<OpenHoursIndividual>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("openHoursIndividual_pk");

            entity.ToTable("openHoursIndividual");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"OpenHoursIndividual_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Days)
                .HasMaxLength(50)
                .HasColumnName("days");
            entity.Property(e => e.Hours)
                .HasMaxLength(50)
                .HasColumnName("hours");
            entity.Property(e => e.OfficeId).HasColumnName("office_id");

            entity.HasOne(d => d.Office).WithMany(p => p.OpenHoursIndividuals)
                .HasForeignKey(d => d.OfficeId)
                .HasConstraintName("OpenHoursIndividual_offices_id_fk");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("services_pk");

            entity.ToTable("services");

            entity.HasIndex(e => e.Name, "name_uk").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
