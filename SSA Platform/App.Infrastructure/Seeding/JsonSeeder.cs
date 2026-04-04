using System.Text.Json;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Seeding;

public static class JsonSeeder
{
    public static void Seed<TEntity, TDto>(
        AppDbContext db,
        string jsonFileName,
        Func<TDto, TEntity> mapToEntity,
        DbSet<TEntity> dbSet
    ) where TEntity : class
    {
        if (dbSet.Any()) return;

        var filePath = Path.Combine(AppContext.BaseDirectory, "Seeding/data", jsonFileName);
        if (!File.Exists(filePath)) return;

        var json = File.ReadAllText(filePath);
        var items = JsonSerializer.Deserialize<List<TDto>>(json);
        if (items == null || !items.Any()) return;

        foreach (var dto in items)
        {
            dbSet.Add(mapToEntity(dto));
        }

        db.SaveChanges();
    }
}