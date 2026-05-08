using System.Net;
using System.Text;
using FluentAssertions;
using TimezoneMeetingCli.Models;
using TimezoneMeetingCli.Services.Geocoding;

namespace TimezoneMeetingCli.UnitTests;

public class OpenMeteoGeocodingServiceTests
{
    [Fact]
    public async Task ResolveLocationAsync_TimezoneId_BypassesHttpLookup()
    {
        var service = CreateThrowingService(new InvalidOperationException("HTTP should not be called for timezone IDs."));

        var result = await service.ResolveLocationAsync(new LocationQuery("Europe/Paris", QueryType.TimezoneId));

        result.Should().BeEquivalentTo(new GeocodedLocation("Europe/Paris", 0, 0, "Europe/Paris"));
    }

    [Fact]
    public async Task ResolveLocationAsync_InvalidJson_ReturnsNull()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
        });

        var result = await service.ResolveLocationAsync(new LocationQuery("London", QueryType.CityName));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveLocationAsync_TimeoutLikeCancellation_ReturnsNull()
    {
        var service = CreateThrowingService(new TaskCanceledException("request timed out"));

        var result = await service.ResolveLocationAsync(new LocationQuery("London", QueryType.CityName));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveLocationAsync_ExplicitCancellation_Propagates()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var service = CreateThrowingService(new TaskCanceledException("caller canceled"));

        var act = () => service.ResolveLocationAsync(new LocationQuery("London", QueryType.CityName), cancellationSource.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    private static OpenMeteoGeocodingService CreateService(HttpResponseMessage response)
    {
        return new OpenMeteoGeocodingService(new HttpClient(new StubHttpMessageHandler((_, _) => response)));
    }

    private static OpenMeteoGeocodingService CreateThrowingService(Exception exception)
    {
        return new OpenMeteoGeocodingService(new HttpClient(new StubHttpMessageHandler((_, _) => throw exception)));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _send;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> send)
        {
            _send = send;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_send(request, cancellationToken));
        }
    }
}