using FluentAssertions;
using NodaTime;
using TimezoneMeetingCli.Models;
using TimezoneMeetingCli.Services.TimeConversion;

namespace TimezoneMeetingCli.UnitTests;

public class TimeConversionTests
{
    [Fact]
    public void ConvertToLocalTime_WithBaseTime_TranslatesCorrectly()
    {
        var service = new NodaTimeConversionService();
        var location = new GeocodedLocation("Tokyo", 35.6895, 139.6917, "Asia/Tokyo");
        
        // Base time: 10:00 AM UTC
        var baseTime = new DateTimeOffset(2026, 5, 8, 10, 0, 0, TimeSpan.Zero);

        var result = service.ConvertToLocalTime(location, baseTime);

        // Tokyo is UTC+9, so 10:00 UTC = 19:00 JST
        result.LocalTime.Hour.Should().Be(19);
        result.UtcOffset.Should().Be(TimeSpan.FromHours(9));
    }
}