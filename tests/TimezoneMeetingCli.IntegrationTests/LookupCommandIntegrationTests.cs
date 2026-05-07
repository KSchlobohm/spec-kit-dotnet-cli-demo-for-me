using FluentAssertions;
using System.Diagnostics;

namespace TimezoneMeetingCli.IntegrationTests;

public class LookupCommandIntegrationTests
{
    [Fact]
    public void Lookup_ZipCode_ReturnsSuccess()
    {
        var process = StartCli("time lookup 10001 --format json");
        process.WaitForExit();

        process.ExitCode.Should().Be(0);
        var output = process.StandardOutput.ReadToEnd();
        output.Should().Contain("New York"); // 10001 resolves to NY
    }
    
    [Fact]
    public void Lookup_BadTarget_ReturnsError()
    {
        var process = StartCli("time lookup ThisIsADefinitelyFakeLocationName");
        process.WaitForExit();

        process.ExitCode.Should().Be(1);
        var error = process.StandardOutput.ReadToEnd(); // Spectre pushes markup to stdout sometimes, let's catch both
        error += process.StandardError.ReadToEnd();
        error.Should().Contain("could not be resolved");
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