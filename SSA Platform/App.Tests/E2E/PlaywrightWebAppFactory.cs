using App.Domain.Entities.Identity;
using App.Infrastructure.Persistence;
using App.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace App.Tests.E2E
{
    public class PlaywrightWebAppFactory : CustomWebApplicationFactory
    {
        private IHost? _kestrelHost;

        public string RootUri => "http://host.docker.internal:5065";

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // ✅ Configure Kestrel BEFORE calling Build()
            builder.ConfigureWebHost(webHost =>
            {
                webHost.UseKestrel();
                webHost.UseUrls("http://0.0.0.0:5065");
            });

            // ✅ Single Build() call — produces the real app host with Kestrel
            _kestrelHost = builder.Build();
            _kestrelHost.Start();

            // Seed database using the Kestrel host's DI container
            using var scope = _kestrelHost.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            AppDataInit.SeedIdentity(userManager, roleManager);

            // ✅ Return a separate minimal TestServer host to satisfy
            // WebApplicationFactory's internal requirement
            var testHost = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.Configure(_ => { });
                })
                .Build();

            testHost.Start();
            return testHost;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _kestrelHost?.Dispose();
            base.Dispose(disposing);
        }
    }
}