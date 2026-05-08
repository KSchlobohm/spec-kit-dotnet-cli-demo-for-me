namespace TimezoneMeetingCli.Models;

public record LocalizedTimeResult(
    string Query,
    string ResolvedLocation,
    DateTimeOffset LocalTime,
    TimeSpan UtcOffset,
    string TimezoneAbbreviation
);