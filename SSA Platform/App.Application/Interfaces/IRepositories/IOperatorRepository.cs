using App.Domain;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IOperatorRepository : IRepository<Operator>
{
    Task<Operator?> GetWithDetailsAsync(Guid id);
}