using System.Text.Json;
using Spectre.Console;

namespace TimezoneMeetingCli.Infrastructure;

public static class OutputHandler
{
    public static void PrintError(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
    }

    public static void PrintJson<T>(T data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(data, options);
        Console.WriteLine(json); // JSON should strictly go to stdout
    }
}