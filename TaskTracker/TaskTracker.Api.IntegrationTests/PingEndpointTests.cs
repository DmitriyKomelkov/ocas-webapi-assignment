using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TaskTracker.Api.IntegrationTests;

public class PingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PingEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("pong", body.Trim('"'));
    }
}
