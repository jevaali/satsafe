using App.Domain.Entities;


namespace App.Application.Interfaces.IRepositories;

public interface ISatelliteRepository : IRepository<Satellite>
{
    Task<IEnumerable<Satellite>> GetByOperatorIdAsync(Guid operatorId);
    Task<int> GetCountAsync();
    Task<int> GetActiveCountAsync();
    Task<List<Satellite>> GetTopAsync(int count);
}
