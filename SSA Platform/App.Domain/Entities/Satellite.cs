using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public class Satellite : SpaceObject
{
    public LangStr Name { get; private set; } = string.Empty;
    public DateTime LaunchDate { get; private set; }
    public ESatelliteStatus Status { get; private set; }

    public Guid OperatorId { get; private set; }
    public required Operator Operator { get; set; }

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
            Orbit = null,
            CreatedBy = "system", 
            CreatedAt = DateTime.UtcNow
        };
    }
    public void UpdateDetails(string name, DateTime launchDate, Operator op, OrbitParameters orbitParams)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        Name = name;
        LaunchDate = launchDate;
        Operator = op;
        OperatorId = op.Id;
    }
    
    public void SoftDelete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = ESatelliteStatus.Deleted;
    }
}