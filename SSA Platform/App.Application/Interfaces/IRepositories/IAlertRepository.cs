using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> GetByConjunctionIdsAsync(List<Guid> conjunctionIds);
}
