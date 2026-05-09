using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IDebrisObjectRepository : IBaseRepository<DebrisObject>
{
    Task<int> GetCountAsync();
}
