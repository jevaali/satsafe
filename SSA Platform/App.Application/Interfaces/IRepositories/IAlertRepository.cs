using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IAlertRepository : IBaseRepository<Alert>
{
    Task<IEnumerable<Alert>> GetByConjunctionIdsAsync(List<Guid> conjunctionIds);
    Task UpdateMessageAsync(Alert entity);
    Task UpdateAcknowledgedAsync(Alert entity);
}
