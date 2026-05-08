using System.Diagnostics;
using System.Text.Json;
using TimezoneMeetingCli.Models;

namespace TimezoneMeetingCli.IntegrationTests;

internal static class CliProcessHelper
{
    private const string GeocodingFixtureEnvironmentVariable = "TIMEZONE_MEETING_CLI_GEOCODING_FIXTURE_JSON";

    public static Process StartCli(string arguments, IReadOnlyDictionary<string, GeocodedLocation>? geocodingFixture = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project ../../../../../src/TimezoneMeetingCli/TimezoneMeetingCli.csproj -- {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (geocodingFixture != null)
        {
            startInfo.EnvironmentVariables[GeocodingFixtureEnvironmentVariable] = JsonSerializer.Serialize(geocodingFixture);
        }

        return Process.Start(startInfo)!;
    }
}