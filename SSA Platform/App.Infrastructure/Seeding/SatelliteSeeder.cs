using System.Text.Json;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.Seeding;

public static class SatelliteSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Satellites.Any()) return;

        var filePath = Path.Combine(AppContext.BaseDirectory, "Seeding", "satellites.json");
        var json = File.ReadAllText(filePath);
        
        var satellites = JsonSerializer.Deserialize<List<SatelliteSeedDto>>(json);

        foreach (var s in satellites!)
        {
            var satellite = Satellite.CreateFromExternalData(
                s.Name,
                s.LaunchDate,
                s.OperatorName,
                s.OrbitParameters
            );
            db.Satellites.Add(satellite);
        }

        db.SaveChanges();
    }
}