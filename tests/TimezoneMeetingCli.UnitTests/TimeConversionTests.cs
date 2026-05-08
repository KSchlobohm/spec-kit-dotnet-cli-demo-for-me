using FluentAssertions;
using NodaTime;
using TimezoneMeetingCli.Commands;
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

    [Fact]
    public void BuildBaseTimeOffset_UsesOffsetForParsedDate()
    {
        var standardOffset = TimeSpan.FromHours(-5);
        var daylightDelta = TimeSpan.FromHours(1);
        var daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 2, 0, 0),
            3,
            2,
            DayOfWeek.Sunday);
        var daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 2, 0, 0),
            11,
            1,
            DayOfWeek.Sunday);
        var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(2000, 1, 1),
            new DateTime(2099, 12, 31),
            daylightDelta,
            daylightTransitionStart,
            daylightTransitionEnd);
        var customTimeZone = TimeZoneInfo.CreateCustomTimeZone(
            "CustomEastern",
            standardOffset,
            "Custom Eastern",
            "Custom Eastern",
            "Custom Eastern Daylight",
            new[] { adjustmentRule });

        var winterMeeting = ScheduleCommand.BuildBaseTimeOffset(new DateTime(2026, 1, 15, 14, 0, 0), customTimeZone);
        var summerMeeting = ScheduleCommand.BuildBaseTimeOffset(new DateTime(2026, 7, 15, 14, 0, 0), customTimeZone);

        winterMeeting.Offset.Should().Be(TimeSpan.FromHours(-5));
        summerMeeting.Offset.Should().Be(TimeSpan.FromHours(-4));
    }

    [Fact]
    public void ConvertToLocalTime_UsesOffsetAgnosticParameterName()
    {
        var interfaceMethod = typeof(ITimeConversionService).GetMethod(nameof(ITimeConversionService.ConvertToLocalTime));
        var implementationMethod = typeof(NodaTimeConversionService).GetMethod(nameof(NodaTimeConversionService.ConvertToLocalTime));

        interfaceMethod.Should().NotBeNull();
        implementationMethod.Should().NotBeNull();
        interfaceMethod!.GetParameters()[1].Name.Should().Be("baseTime");
        implementationMethod!.GetParameters()[1].Name.Should().Be("baseTime");
    }
}