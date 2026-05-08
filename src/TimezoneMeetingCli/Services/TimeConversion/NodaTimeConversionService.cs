using NodaTime;
using TimeZoneConverter;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Services.TimeConversion;

public class NodaTimeConversionService : ITimeConversionService
{
    public LocalizedTimeResult ConvertToLocalTime(GeocodedLocation location, DateTimeOffset? baseTime = null)
    {
        var inputDateTime = baseTime ?? DateTimeOffset.UtcNow;
        var instant = Instant.FromDateTimeOffset(inputDateTime);

        var tzId = location.TimezoneId;
        
        // Handle Windows to IANA conversion if needed
        if (TZConvert.TryWindowsToIana(tzId, out var ianaId))
        {
            tzId = ianaId;
        }

        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId) ?? DateTimeZone.Utc;
        var zonedDateTime = instant.InZone(dateTimeZone);

        return new LocalizedTimeResult(
            Query: location.Name,
            ResolvedLocation: location.Name,
            LocalTime: zonedDateTime.ToDateTimeOffset(),
            UtcOffset: zonedDateTime.Offset.ToTimeSpan(),
            TimezoneAbbreviation: zonedDateTime.Zone.Id
        );
    }
}