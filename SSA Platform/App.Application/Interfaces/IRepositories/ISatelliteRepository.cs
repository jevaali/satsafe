using App.Domain.Entities;
using Base.Contracts;


namespace App.Application.Interfaces.IRepositories;

public interface ISatelliteRepository : IBaseRepository<Satellite>
{
    Task<IEnumerable<Satellite>> GetByOperatorIdAsync(Guid operatorId);
    Task<int> GetCountAsync();
    Task<int> GetActiveCountAsync();
    Task<List<Satellite>> GetTopAsync(int count);
    Task AddTleRecordAsync(TleRecord record);
}
