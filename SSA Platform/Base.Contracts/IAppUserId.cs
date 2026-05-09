namespace Base.Contracts;

public interface IAppUserId : IAppUserId<Guid>
{
}

public interface IAppUserId<TKey>
    where TKey: IEquatable<TKey>
{
    public TKey AppUserId { get; set; }
}