namespace TimezoneMeetingCli.Models;

public enum QueryType
{
    ZipCode,
    CityName,
    TimezoneId
}

public record LocationQuery(string RawInput, QueryType QueryType);
