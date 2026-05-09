using App.Application.Interfaces;
using Base.Contracts;

namespace App.Application;

public abstract class BaseUseCase
{
    protected readonly IUnitOfWork Uow;

    protected BaseUseCase(IUnitOfWork uow)
    {
        Uow = uow;
    }
}

public abstract class BaseUseCase<TDto, TEntity> : BaseUseCase
    where TDto : class
    where TEntity : class
{
    protected readonly IBaseMapper<TDto, TEntity> Mapper;

    protected BaseUseCase(IUnitOfWork uow, IBaseMapper<TDto, TEntity> mapper) : base(uow)
    {
        Mapper = mapper;
    }
}
