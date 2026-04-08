using App.Domain.Entities.Identity;
using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public class DraftSatellite : BaseEntityWithMeta
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    
    public string Name { get; set; }
    public string? Notes { get; set; }
    public OrbitParameters Parameters { get; set; }
}