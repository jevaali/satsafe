using App.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebApp.Tests.E2E;

public class PlaywrightWebAppFactory : CustomWebApplicationFactory
{
    private IHost? _kestrelHost;

    public string RootUri => "http://host.docker.internal:5065";

    public PlaywrightWebAppFactory()
    {
        _ = Services;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        builder.ConfigureWebHost(webHost =>
        {
            webHost.UseKestrel();
            webHost.UseUrls("http://0.0.0.0:5065");
        });

        _kestrelHost = builder.Build();

        // Seed the Kestrel host — same schema + roles + data
        EnsureCreatedAndSeed(_kestrelHost.Services);

        _kestrelHost.Start();
        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _kestrelHost?.Dispose();
        base.Dispose(disposing);
    }
}