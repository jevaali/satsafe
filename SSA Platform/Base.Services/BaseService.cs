using Base.Contracts;
using Base.Services.Contracts;

namespace Base.Services;

public class
    BaseService<TBLLEntity, TDALEntity, TRepository, TUOW> :
    BaseService<Guid, TBLLEntity, TDALEntity, TRepository, TUOW> where TBLLEntity : class, IBaseEntity<Guid>
    where TDALEntity : class, IBaseEntity<Guid>
    where TRepository : IBaseRepository<Guid, TDALEntity>
    where TUOW: IBaseUOW
{
    public BaseService(TRepository serviceRepository, TUOW serviceBLL, IBaseMapper<TBLLEntity, TDALEntity> mapper) :
        base(serviceRepository, serviceBLL, mapper)
    {
    }
}

public class BaseService<TKey, TBLLEntity, TDALEntity, TRepository, TUOW> : IBaseService<TKey, TBLLEntity>
    where TBLLEntity : class, IBaseEntity<TKey>
    where TKey : IEquatable<TKey>
    where TRepository : IBaseRepository<TKey, TDALEntity>
    where TDALEntity : class, IBaseEntity<TKey>
    where TUOW: IBaseUOW
{
    protected readonly TUOW ServiceUOW;
    protected readonly TRepository ServiceRepository;
    protected readonly IBaseMapper<TBLLEntity, TDALEntity> Mapper;

    public BaseService(TRepository serviceRepository, TUOW serviceUOW, IBaseMapper<TBLLEntity, TDALEntity> mapper)
    {
        ServiceUOW = serviceUOW;
        ServiceRepository = serviceRepository;
        Mapper = mapper;
    }


    public virtual async Task<IEnumerable<TBLLEntity>> AllAsync(TKey appUserId = default!)
    {
        var res = await ServiceRepository.AllAsync(appUserId);
        var mappedRes = res.Select(e => Mapper.Map(e)!).ToList();
        return mappedRes;
    }

    public virtual async Task<TBLLEntity?> FindAsync(TKey id, TKey appUserId = default!)
    {
        var res = await ServiceRepository.FindAsync(id, appUserId);
        return Mapper.Map(res);
    }

    public virtual void Add(TBLLEntity entity, TKey appUserId = default!)
    {
        ServiceRepository.Add(Mapper.Map(entity)!, appUserId);
    }

    public virtual async Task<TBLLEntity> UpdateAsync(TBLLEntity entity, TKey appUserId = default!)
    {
        var res = await ServiceRepository.UpdateAsync(Mapper.Map(entity)!, appUserId);
        return Mapper.Map(res)!;
    }

    public virtual void Remove(TBLLEntity entity)
    {
        ServiceRepository.Remove(Mapper.Map(entity)!);
    }

    public virtual async Task RemoveAsync(TKey id, TKey appUserId = default!)
    {
        await ServiceRepository.RemoveAsync(id, appUserId);
    }
}