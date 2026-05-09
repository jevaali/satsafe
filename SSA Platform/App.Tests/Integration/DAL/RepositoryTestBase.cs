using App.Domain.Entities.Identity;
using App.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.Integration.DAL
{
    public abstract class RepositoryTestBase : IAsyncLifetime
    {
        protected readonly SqliteConnection Connection;
        protected readonly DbContextOptions<AppDbContext> ContextOptions;

        protected RepositoryTestBase()
        {
            Connection = new SqliteConnection("DataSource=:memory:");
            Connection.Open();

            ContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Connection)
                // Match Program.cs production tracking behavior; avoids identity conflicts
                // when BaseRepository.Remove maps a fresh Domain instance after FindAsync loaded one.
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                .Options;
        }

        public async Task InitializeAsync()
        {
            await using var ctx = new AppDbContext(ContextOptions);
            await ctx.Database.EnsureCreatedAsync();

            // Insert two AppUsers directly — satisfies FK constraints on user-owned entities.
            ctx.Users.Add(new AppUser
            {
                Id = Helpers.TestUsers.UserAId,
                Email = Helpers.TestUsers.UserAEmail,
                UserName = Helpers.TestUsers.UserAEmail,
                NormalizedEmail = Helpers.TestUsers.UserAEmail.ToUpperInvariant(),
                NormalizedUserName = Helpers.TestUsers.UserAEmail.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
            });
            ctx.Users.Add(new AppUser
            {
                Id = Helpers.TestUsers.UserBId,
                Email = Helpers.TestUsers.UserBEmail,
                UserName = Helpers.TestUsers.UserBEmail,
                NormalizedEmail = Helpers.TestUsers.UserBEmail.ToUpperInvariant(),
                NormalizedUserName = Helpers.TestUsers.UserBEmail.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
            });
            await ctx.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            Connection.Close();
            Connection.Dispose();
            return Task.CompletedTask;
        }

        protected AppDbContext CreateContext() => new(ContextOptions);
        
    }
}
