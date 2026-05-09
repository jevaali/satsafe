using System.Reflection;
using Base.Contracts;
using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.Application.EF;

public class
    BaseRepository<TDALEntity, TDomainEntity, TDbContext> : BaseRepository<Guid, TDALEntity, TDomainEntity, TDbContext>
    where TDALEntity : class, IBaseEntity<Guid>
    where TDomainEntity : class, IBaseEntity<Guid>
    where TDbContext : DbContext
{
    public BaseRepository(TDbContext repositoryDbContext, IBaseMapper<TDALEntity, TDomainEntity> mapper) : base(
        repositoryDbContext, mapper)
    {
    }
}

public class BaseRepository<TKey, TDALEntity, TDomainEntity, TDbContext> : IBaseRepository<TKey, TDALEntity>
    where TDALEntity : class, IBaseEntity<TKey>
    where TDomainEntity : class, IBaseEntity<TKey>
    where TKey : IEquatable<TKey>
    where TDbContext : DbContext
{
    protected readonly TDbContext RepositoryDbContext;
    protected readonly DbSet<TDomainEntity> RepositoryDbSet;
    protected readonly IBaseMapper<TDALEntity, TDomainEntity> Mapper;

    public BaseRepository(TDbContext repositoryDbContext, IBaseMapper<TDALEntity, TDomainEntity> mapper)
    {
        RepositoryDbContext = repositoryDbContext;
        Mapper = mapper;
        RepositoryDbSet = RepositoryDbContext.Set<TDomainEntity>();
    }

    public virtual async Task<IEnumerable<TDALEntity>> AllAsync(TKey appUserId = default!)
    {
        var query = RepositoryDbSet.AsQueryable();

        if (!appUserId.Equals(default) && typeof(IAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
        {
            query = query.Where(e => ((IAppUserId<TKey>)e).AppUserId.Equals(appUserId));
        }

        var domainRes = await query.ToListAsync();
        var res = domainRes.Select(e => Mapper.Map(e)!);
        return res;
    }

    public virtual async Task<TDALEntity?> FindAsync(TKey id, TKey appUserId = default!)
    {
        var query = RepositoryDbSet
            .Where(e => e.Id.Equals(id))
            .AsQueryable();

        if (!appUserId.Equals(default) && typeof(IAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
        {
            query = query.Where(e => ((IAppUserId<TKey>)e).AppUserId.Equals(appUserId));
        }

        var resDomain = await query.FirstOrDefaultAsync();
        var res = Mapper.Map(resDomain);
        return res;
    }

    public virtual void Add(TDALEntity entity, TKey appUserId = default!)
    {
        var domainEntity = Mapper.Map(entity)!;

        if (!appUserId.Equals(default) && typeof(IAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
        {
            ((IAppUserId<TKey>)domainEntity).AppUserId = appUserId;
        }

        RepositoryDbSet.Add(domainEntity);
    }

    private static Dictionary<string, PropertyInfo>? _domainEntityTypePropsLangStr = null;

    public virtual async Task<TDALEntity> UpdateAsync(TDALEntity entity, TKey appUserId = default!)
    {
        var domainEntityType = typeof(TDomainEntity);

        _domainEntityTypePropsLangStr ??= domainEntityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(LangStr))
            .ToDictionary(p => p.Name, StringComparer.Ordinal);

        if (_domainEntityTypePropsLangStr.Count > 0)
        {
            throw new ApplicationException("Override update method for LangStr containing entities - " +
                                           typeof(TDomainEntity).FullName);
        }

        var domainEntity = Mapper.Map(entity)!;

        if (!appUserId.Equals(default) && typeof(IAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
        {
            ((IAppUserId<TKey>)domainEntity!).AppUserId = appUserId;
        }

        return Mapper.Map(
            RepositoryDbSet.Update(
                domainEntity!
            ).Entity
        )!;
    }

    public virtual void Remove(TDALEntity entity)
    {
        RepositoryDbSet.Remove(Mapper.Map(entity)!);
    }

    public virtual async Task RemoveAsync(TKey id, TKey appUserId = default!)
    {
        TDALEntity? entity = null;
        if (!appUserId.Equals(default) && typeof(IAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
        {
            entity = await FindAsync(id, appUserId);
        }
        else
        {
            entity = await FindAsync(id);
        }

        if (entity != null) Remove(entity);
    }
}