using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IDebrisObjectRepository : IRepository<DebrisObject>
{
    Task<int> GetCountAsync();
}
