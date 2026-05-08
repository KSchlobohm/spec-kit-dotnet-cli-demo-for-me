using FluentAssertions;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.IntegrationTests;

public class LookupCommandIntegrationTests
{
    [Fact]
    public void Lookup_ZipCode_ReturnsSuccess()
    {
        var process = CliProcessHelper.StartCli(
            "time lookup Fixture-London --format json",
            new Dictionary<string, GeocodedLocation>
            {
                ["Fixture-London"] = new("London, England, United Kingdom", 51.5072, -0.1276, "Europe/London")
            });
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().Contain("Fixture-London");
        output.Should().Contain("London, England, United Kingdom");
        error.Should().BeEmpty();
    }

    [Fact]
    public void Lookup_IanaTimezoneId_ReturnsSuccess()
    {
        var process = CliProcessHelper.StartCli("time lookup Europe/Paris --format json", new Dictionary<string, GeocodedLocation>());
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().Contain("Europe/Paris");
        error.Should().BeEmpty();
    }

    [Fact]
    public void Lookup_WindowsTimezoneId_ReturnsSuccess()
    {
        var process = CliProcessHelper.StartCli("time lookup \"Eastern Standard Time\" --format json", new Dictionary<string, GeocodedLocation>());
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().Contain("Eastern Standard Time");
        output.Should().Contain("America/New_York");
        error.Should().BeEmpty();
    }
    
    [Fact]
    public void Lookup_BadTarget_ReturnsError()
    {
        var process = CliProcessHelper.StartCli(
            "time lookup Fixture-Missing",
            new Dictionary<string, GeocodedLocation>
            {
                ["Fixture-London"] = new("London, England, United Kingdom", 51.5072, -0.1276, "Europe/London")
            });
        process.WaitForExit();

        process.ExitCode.Should().Be(1);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        output.Should().BeEmpty();
        error.Should().Contain("could not be resolved");
    }
}