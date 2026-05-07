using FluentAssertions;
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
}