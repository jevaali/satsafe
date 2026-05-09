using Base.Contracts;

namespace App.Application.Mappers;

public class IdentityMapper<TOut, TIn> : IBaseMapper<TOut, TIn>
    where TOut : class
    where TIn : class
{
    public TOut? Map(TIn? entity) => entity as TOut;
    public TIn? Map(TOut? entity) => entity as TIn;
}

public class IdentityMapper<T> : IdentityMapper<T, T> where T : class { }
