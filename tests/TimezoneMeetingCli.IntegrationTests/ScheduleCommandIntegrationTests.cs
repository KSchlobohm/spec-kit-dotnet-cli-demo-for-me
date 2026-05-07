using FluentAssertions;
using System.Diagnostics;

namespace TimezoneMeetingCli.IntegrationTests;

public class ScheduleCommandIntegrationTests
{
    [Fact]
    public void Schedule_Locations_ReturnsSuccess()
    {
        var process = StartCli("time schedule 14:00 10001 London Tokyo Sydney Berlin --format json");
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        
        output.Should().Contain("10001");
        output.Should().Contain("London");
        output.Should().Contain("Tokyo");
        output.Should().Contain("Sydney");
        output.Should().Contain("Berlin");
    }
    
    [Fact]
    public void Schedule_BadTarget_GracefullySkipsAndReturnsSuccess()
    {
        var process = StartCli("time schedule 14:00 ThisIsADefinitelyFakeLocationName London");
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var error = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        error.Should().Contain("could not be resolved. Skipping");
        
        var output = error;
        output.Should().Contain("London");
    }

    private Process StartCli(string arguments)
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

        return Process.Start(startInfo)!;
    }
}