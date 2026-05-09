using Microsoft.AspNetCore.Mvc.Testing;

namespace App.Tests.Integration;

public class HomeControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public CustomWebApplicationFactory _factory { get; set; }

    public HomeControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            }
        );
    }


    [Fact]
    public async Task HomeIndex()
    {
        // act
        var response = await _client.GetAsync("/");
        
        // assert
        response.EnsureSuccessStatusCode();
    }
    
}