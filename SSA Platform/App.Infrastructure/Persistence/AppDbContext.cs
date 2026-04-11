using System.Text.Json;
using App.Domain;
using App.Domain.Entities;
using App.Domain.Entities.Identity;
using Base.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

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
    public DbSet<SpaceObject> SpaceObjects { get; set; } = null!;
    public DbSet<TleRecord> TleRecords { get; set; } = null!;
    public DbSet<UserOperator> UserOperators { get; set; } = null!;
    
    public DbSet<AppRefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<DraftSatellite> DraftSatellites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SpaceObject>()
            .HasDiscriminator<string>("SpaceObjectType")
            .HasValue<Satellite>("Satellite")
            .HasValue<DebrisObject>("DebrisObject");
        
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
            .HasForeignKey<Orbit>(o => o.SpaceObjectId);

        modelBuilder.Entity<TleRecord>()
            .HasOne(t => t.SpaceObject)
            .WithMany(s => s.TleRecords)
            .HasForeignKey(t => t.SpaceObjectId);
        
        modelBuilder.Entity<Orbit>(entity =>
        {
            entity.OwnsOne(o => o.Parameters, p =>
            {
                p.Property(pp => pp.SemiMajorAxis).HasColumnName("SemiMajorAxis");
                p.Property(pp => pp.Eccentricity).HasColumnName("Eccentricity");
                p.Property(pp => pp.Inclination).HasColumnName("Inclination");
                p.Property(pp => pp.RAAN).HasColumnName("RAAN");
                p.Property(pp => pp.ArgumentOfPerigee).HasColumnName("ArgumentOfPerigee");
                p.Property(pp => pp.MeanAnomaly).HasColumnName("MeanAnomaly");
            });
        });
        
        modelBuilder.Entity<Alert>().Property(e => e.Message)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");
        
        
        modelBuilder.Entity<Operator>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");
        
        
        modelBuilder.Entity<Operator>().Property(e => e.OperatorTypeString)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");
        
        modelBuilder.Entity<Satellite>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LangStr>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<DraftSatellite>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.OwnsOne(d => d.Parameters, p =>
            {
                p.Property(pp => pp.SemiMajorAxis).HasColumnName("SemiMajorAxis");
                p.Property(pp => pp.Eccentricity).HasColumnName("Eccentricity");
                p.Property(pp => pp.Inclination).HasColumnName("Inclination");
                p.Property(pp => pp.RAAN).HasColumnName("RAAN");
                p.Property(pp => pp.ArgumentOfPerigee).HasColumnName("ArgumentOfPerigee");
                p.Property(pp => pp.MeanAnomaly).HasColumnName("MeanAnomaly");
            });
        });
    }
}