using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;

namespace App.Domain.Entities;

public class Satellite : SpaceObject
{
    public string Name { get; private set; } = string.Empty;
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
            Orbit = null
        };
    }

    public void SoftDelete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = ESatelliteStatus.Deleted;
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
    public void SetOrbit(OrbitParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        if (Orbit == null)
        {
            Orbit = new Orbit(parameters)
            {
                SpaceObject = this,
                SpaceObjectId = this.Id
            };
        }
        else
        {
            Orbit.UpdateParameters(parameters);
        }
    }
}