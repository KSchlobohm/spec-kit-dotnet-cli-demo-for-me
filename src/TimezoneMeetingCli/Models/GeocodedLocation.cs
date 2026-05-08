namespace TimezoneMeetingCli.Models;

public record GeocodedLocation(
    string Name,
    double Latitude,
    double Longitude,
    string TimezoneId
);