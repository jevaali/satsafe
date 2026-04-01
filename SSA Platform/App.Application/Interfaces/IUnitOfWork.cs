using App.Application.Interfaces.IRepositories;

namespace App.Application.Interfaces;

public interface IUnitOfWork
{
    ISatelliteRepository Satellites { get; }
    IOperatorRepository Operators { get; }

    Task CommitAsync();
}