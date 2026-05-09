namespace Base.Contracts;

public interface IBaseUOW
{
    Task<int> CommitAsync();
}