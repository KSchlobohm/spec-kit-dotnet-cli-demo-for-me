using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.Services.TimeConversion;

public interface ITimeConversionService
{
    LocalizedTimeResult ConvertToLocalTime(GeocodedLocation location, DateTimeOffset? baseTime = null);
}