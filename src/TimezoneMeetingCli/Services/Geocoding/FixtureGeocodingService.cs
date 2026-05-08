using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Services.Geocoding;

public class FixtureGeocodingService : IGeocodingService
{
    private readonly IReadOnlyDictionary<string, GeocodedLocation> _fixtures;

    public FixtureGeocodingService(IReadOnlyDictionary<string, GeocodedLocation> fixtures)
    {
        _fixtures = fixtures;
    }

    public Task<GeocodedLocation?> ResolveLocationAsync(LocationQuery query, CancellationToken cancellationToken = default)
    {
        if (query.QueryType == QueryType.TimezoneId)
        {
            return Task.FromResult<GeocodedLocation?>(new GeocodedLocation(query.RawInput, 0, 0, query.RawInput));
        }

        _fixtures.TryGetValue(query.RawInput, out var location);
        return Task.FromResult<GeocodedLocation?>(location);
    }
}