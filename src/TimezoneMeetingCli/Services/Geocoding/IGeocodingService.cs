using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Services.Geocoding;

public interface IGeocodingService
{
    Task<GeocodedLocation?> ResolveLocationAsync(LocationQuery query, CancellationToken cancellationToken = default);
}