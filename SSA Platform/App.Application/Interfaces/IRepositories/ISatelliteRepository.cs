using App.Domain;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface ISatelliteRepository
{
    Task AddAsync(Satellite satellite);
    Task<Satellite> GetByIdAsync(Guid satelliteId);
    Task Update(Satellite satellite);
}