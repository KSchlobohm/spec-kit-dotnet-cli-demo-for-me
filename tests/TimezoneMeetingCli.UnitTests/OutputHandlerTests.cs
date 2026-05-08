using FluentAssertions;
using TimezoneMeetingCli.Infrastructure;

namespace TimezoneMeetingCli.UnitTests;

public class OutputHandlerTests
{
    [Fact]
    public void PrintError_WritesOnlyToStandardError()
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();

        try
        {
            Console.SetOut(standardOutput);
            Console.SetError(standardError);

            OutputHandler.PrintError("bad input");
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        standardOutput.ToString().Should().BeEmpty();
        standardError.ToString().Should().Contain("Error: bad input");
    }
}