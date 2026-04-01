using App.Domain;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IOperatorRepository
{
    Task<Operator?> GetByIdAsync(Guid id);
}