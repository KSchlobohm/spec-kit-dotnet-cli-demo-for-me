using FluentAssertions;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.IntegrationTests;

public class ScheduleCommandIntegrationTests
{
    [Fact]
    public void Schedule_Locations_ReturnsSuccess()
    {
        var process = CliProcessHelper.StartCli(
            "time schedule 14:00 Fixture-NewYork Fixture-London Fixture-Tokyo Fixture-Sydney Fixture-Berlin --format json",
            new Dictionary<string, GeocodedLocation>
            {
                ["Fixture-NewYork"] = new("New York, NY, USA", 40.7506, -73.9972, "America/New_York"),
                ["Fixture-London"] = new("London, England, United Kingdom", 51.5072, -0.1276, "Europe/London"),
                ["Fixture-Tokyo"] = new("Tokyo, Japan", 35.6764, 139.65, "Asia/Tokyo"),
                ["Fixture-Sydney"] = new("Sydney, NSW, Australia", -33.8688, 151.2093, "Australia/Sydney"),
                ["Fixture-Berlin"] = new("Berlin, Germany", 52.52, 13.405, "Europe/Berlin")
            });
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        output.Should().Contain("Fixture-NewYork");
        output.Should().Contain("Fixture-London");
        output.Should().Contain("Fixture-Tokyo");
        output.Should().Contain("Fixture-Sydney");
        output.Should().Contain("Fixture-Berlin");
        error.Should().BeEmpty();
    }
    
    [Fact]
    public void Schedule_BadTarget_GracefullySkipsAndReturnsSuccess()
    {
        var process = CliProcessHelper.StartCli(
            "time schedule 14:00 Fixture-Missing Fixture-London Fixture-Tokyo Fixture-Sydney Fixture-Berlin Fixture-Paris",
            new Dictionary<string, GeocodedLocation>
            {
                ["Fixture-London"] = new("London, England, United Kingdom", 51.5072, -0.1276, "Europe/London"),
                ["Fixture-Tokyo"] = new("Tokyo, Japan", 35.6764, 139.65, "Asia/Tokyo"),
                ["Fixture-Sydney"] = new("Sydney, NSW, Australia", -33.8688, 151.2093, "Australia/Sydney"),
                ["Fixture-Berlin"] = new("Berlin, Germany", 52.52, 13.405, "Europe/Berlin"),
                ["Fixture-Paris"] = new("Paris, France", 48.8566, 2.3522, "Europe/Paris")
            });
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().Contain("Fixture-London");
        error.Should().Contain("could not be resolved. Skipping");
    }

    [Fact]
    public void Schedule_FewerThanFiveLocations_ReturnsError()
    {
        var process = CliProcessHelper.StartCli("time schedule 14:00 Fixture-London Fixture-Tokyo Fixture-Sydney Fixture-Berlin");
        process.WaitForExit();

        process.ExitCode.Should().Be(1);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().BeEmpty();
        error.Should().Contain("At least 5 locations must be provided");
    }
}