using FluentAssertions;
using TimezoneMeetingCli.Commands;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.UnitTests;

public class LocationQueryParsingTests
{
    [Fact]
    public void Constructor_SetsProperties_Correctly()
    {
        var query = new LocationQuery("10001", QueryType.ZipCode);

        query.RawInput.Should().Be("10001");
        query.QueryType.Should().Be(QueryType.ZipCode);
    }

    [Theory]
    [InlineData("10001")]
    [InlineData("90210")]
    public void DetectQueryType_ZipCode_ReturnsZipCode(string input)
    {
        LocationQueryParser.DetectQueryType(input).Should().Be(QueryType.ZipCode);
    }

    [Theory]
    [InlineData("Europe/Paris")]
    [InlineData("America/New_York")]
    [InlineData("Asia/Tokyo")]
    public void DetectQueryType_IanaTimezoneId_ReturnsTimezoneId(string input)
    {
        LocationQueryParser.DetectQueryType(input).Should().Be(QueryType.TimezoneId);
    }

    [Theory]
    [InlineData("Eastern Standard Time")]
    [InlineData("Pacific Standard Time")]
    public void DetectQueryType_WindowsTimezoneId_ReturnsTimezoneId(string input)
    {
        LocationQueryParser.DetectQueryType(input).Should().Be(QueryType.TimezoneId);
    }

    [Theory]
    [InlineData("London")]
    [InlineData("New York")]
    [InlineData("Berlin")]
    public void DetectQueryType_CityName_ReturnsCityName(string input)
    {
        LocationQueryParser.DetectQueryType(input).Should().Be(QueryType.CityName);
    }
}