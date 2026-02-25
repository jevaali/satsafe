using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<Alert> Alerts { get; set; } = null!;
    public DbSet<ConjunctionEvent> ConjunctionEvents { get; set; } = null!;
    public DbSet<DebrisObject> DebrisObjects { get; set; } = null!;
    public DbSet<NotificationSetting> NotificationSettings { get; set; } = null!;
    public DbSet<Operator> Operators { get; set; } = null!;
    public DbSet<Orbit> Orbits { get; set; } = null!;
    public DbSet<RiskAssessment> RiskAssessments { get; set; } = null!;
    public DbSet<Satellite> Satellites { get; set; } = null!;
    public DbSet<TleRecord> TleRecords { get; set; } = null!;
    public DbSet<UserOperator> UserOperators { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table-per-hierarchy (TPH) inheritance for SpaceObject
        modelBuilder.Entity<SpaceObject>()
            .HasDiscriminator<string>("ObjectType")
            .HasValue<Satellite>("Satellite")
            .HasValue<DebrisObject>("Debris");

        // Configure relationships
        modelBuilder.Entity<ConjunctionEvent>()
            .HasOne(ce => ce.PrimaryObject)
            .WithMany(d => d.PrimaryConjunctionEvents)
            .HasForeignKey(ce => ce.PrimaryObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConjunctionEvent>()
            .HasOne(ce => ce.SecondaryObject)
            .WithMany(d => d.SecondaryConjunctionEvents)
            .HasForeignKey(ce => ce.SecondaryObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConjunctionEvent>()
            .HasOne(ce => ce.RiskAssessment)
            .WithOne(ra => ra.ConjunctionEvent)
            .HasForeignKey<RiskAssessment>(ra => ra.ConjunctionEventId);
        
        modelBuilder.Entity<SpaceObject>()
            .HasOne(s => s.Orbit)
            .WithOne(o => o.SpaceObject)
            .HasForeignKey<SpaceObject>(s => s.OrbitId);

        modelBuilder.Entity<TleRecord>()
            .HasOne(t => t.SpaceObject)
            .WithMany(s => s.TleRecords)
            .HasForeignKey(t => t.SpaceObjectId);
    }
}