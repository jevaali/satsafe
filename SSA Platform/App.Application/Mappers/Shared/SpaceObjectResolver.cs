using App.Domain.Entities;

namespace App.Application.Mappers.Shared;

internal static class SpaceObjectResolver
{
    public static string ResolveName(SpaceObject? obj, Guid fallbackId) => obj switch
    {
        Satellite s    => s.Name.ToString(),
        DebrisObject d => $"DEBRIS-{d.NoradId}",
        _              => fallbackId.ToString()
    };

    public static int? ResolveNoradId(SpaceObject? obj) => obj switch
    {
        Satellite s    => s.NoradId,
        DebrisObject d => d.NoradId,
        _              => null
    };
}