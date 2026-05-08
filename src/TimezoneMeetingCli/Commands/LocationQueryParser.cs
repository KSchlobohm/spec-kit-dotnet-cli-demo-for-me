using NodaTime;
using TimeZoneConverter;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Commands;

public static class LocationQueryParser
{
    /// <summary>
    /// Detects the <see cref="QueryType"/> for a raw location string.
    /// Recognises US ZIP codes (all-digit), IANA timezone IDs (e.g. "Europe/Paris"),
    /// and Windows timezone names (e.g. "Eastern Standard Time"); everything else is
    /// treated as a city/place name.
    /// </summary>
    public static QueryType DetectQueryType(string input)
    {
        if (int.TryParse(input, out _))
            return QueryType.ZipCode;

        if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(input) != null)
            return QueryType.TimezoneId;

        if (TZConvert.TryWindowsToIana(input, out _))
            return QueryType.TimezoneId;

        return QueryType.CityName;
    }
}
