using App.Domain;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IOperatorRepository : IBaseRepository<Operator>
{
    Task<Operator?> GetWithDetailsAsync(Guid id);
}