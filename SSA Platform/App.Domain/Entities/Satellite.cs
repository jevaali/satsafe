using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public class Satellite : SpaceObject
{
    public LangStr Name { get; private set; } = string.Empty;
    public DateTime LaunchDate { get; private set; }
    public ESatelliteStatus Status { get; private set; }

    public Guid? OperatorId { get; private set; }
    public Operator? Operator { get; set; }

    public RiskAssessment? RiskAssessment { get; set; }
    
    public string? OperatorName { get; private set; } = string.Empty;

    private Satellite() { }

    public static Satellite Create(
        string name,
        DateTime launchDate,
        Operator op)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new Satellite
        {
            Id = Guid.NewGuid(),
            Name = name,
            LaunchDate = launchDate,
            Status = ESatelliteStatus.Active,
            Operator = op,
            OperatorId = op.Id,
            OperatorName = op.Name,
            Orbit = null,
            CreatedBy = "system", 
            CreatedAt = DateTime.UtcNow
        };
    }
    public void UpdateDetails(string name, DateTime launchDate, Operator op, OrbitParameters orbitParams, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        Name.SetTranslation(name);
        LaunchDate = launchDate;
        Operator = op;
        OperatorId = op.Id;
        SetOrbit(orbitParams);
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = ESatelliteStatus.Deleted;
    }

    public static Satellite CreateFromExternalData(string sName,
        DateTime sLaunchDate,
        string? sOperatorName,
        OrbitParameters? sOrbitParameters)
    {
        if (string.IsNullOrWhiteSpace(sName))
            throw new ArgumentException("Satellite name is required", nameof(sName));

        var satellite = new Satellite
        {
            Id = Guid.NewGuid(),
            Name = sName,
            LaunchDate = sLaunchDate,
            Status = ESatelliteStatus.Active,
            Operator = null,
            OperatorId = null,
            OperatorName = sOperatorName ?? string.Empty,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow,
            Orbit = null
        };
        
        if (sOrbitParameters != null)
        {
            satellite.SetOrbit(sOrbitParameters);
        }

        return satellite;
    }
}