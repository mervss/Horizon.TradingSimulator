using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using Horizon.Core.Domain;

public class RestIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RestIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Latest_Returns_Deterministic_Test_Tick()
    {
        
        await Task.Delay(300);

        var res = await _client.GetAsync("/api/prices/AAPL");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var tick = await res.Content.ReadFromJsonAsync<PriceTickV1>();
        tick!.Symbol.Should().Be("AAPL");
        tick.Price.Should().BeInRange(100m, 1000m);
    }

    [Fact]
    public async Task History_Has_At_Most_Ten_Items()
    {
        // bir iki tur üretim için kısa bekleme
        await Task.Delay(TimeSpan.FromSeconds(6));

        var res = await _client.GetAsync("/api/prices/AAPL/history");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await res.Content.ReadFromJsonAsync<dynamic[]>();
        list!.Length.Should().BeLessThanOrEqualTo(10);
    }
    [Fact]
    public async Task Unknown_Symbol_History_Returns_Empty_List()
    {
        var res = await _client.GetAsync("/api/prices/XXXX/history");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await res.Content.ReadFromJsonAsync<List<object>>();
        list!.Count.Should().Be(0);
    }
}
