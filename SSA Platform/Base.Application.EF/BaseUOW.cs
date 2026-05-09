using Base.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Base.EF;

public class BaseUOW<TDbContext>: IBaseUOW
    where TDbContext : DbContext

{
    protected readonly TDbContext UowDbContext;

    public BaseUOW(TDbContext dbContext)
    {
        UowDbContext = dbContext;
    }

    public async Task<int> CommitAsync()
    {
        return await UowDbContext.SaveChangesAsync();
    }
}