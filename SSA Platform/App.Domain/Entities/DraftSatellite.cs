using App.Domain.Entities.Identity;
using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public class DraftSatellite : BaseEntityWithMeta
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public OrbitParameters Parameters { get; set; } = default!;

    private DraftSatellite() { }

    public static DraftSatellite Create(Guid userId, string name, string? notes, OrbitParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new DraftSatellite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Notes = notes?.Trim(),
            Parameters = parameters,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? notes, OrbitParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        Notes = notes?.Trim();
        Parameters = parameters;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = UserId.ToString();
    }
}
