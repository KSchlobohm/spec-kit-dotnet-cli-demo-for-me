using System.ComponentModel;
using Spectre.Console.Cli;
using TimezoneMeetingCli.Infrastructure;
using TimezoneMeetingCli.Models;
using TimezoneMeetingCli.Services.Geocoding;
using TimezoneMeetingCli.Services.TimeConversion;
using Spectre.Console;

namespace TimezoneMeetingCli.Commands;

public class LookupCommand : AsyncCommand<LookupCommand.Settings>
{
    private readonly IGeocodingService _geocodingService;
    private readonly ITimeConversionService _timeConversionService;

    public LookupCommand(IGeocodingService geocodingService, ITimeConversionService timeConversionService)
    {
        _geocodingService = geocodingService;
        _timeConversionService = timeConversionService;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<location>")]
        [Description("The target location (e.g., \"10001\", \"London\", \"PST\").")]
        public string Location { get; set; } = string.Empty;

        [CommandOption("--format")]
        [Description("Output format, defaults to text. Valid options: text, json.")]
        [DefaultValue("text")]
        public string Format { get; set; } = "text";
    }

    // The base method includes CancellationToken in Spectre.Console higher versions.
    // Changing signature to override appropriately.
    protected override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        return ExecuteInternalAsync(context, settings);
    }
    private async Task<int> ExecuteInternalAsync(CommandContext context, Settings settings)
    {
        var isZipCode = int.TryParse(settings.Location, out _);
        var queryType = isZipCode ? QueryType.ZipCode : QueryType.CityName;
        // Edge cases could map standard abbreviations to TimezoneId directly, or let the Geocoder handle it.

        var locationQuery = new LocationQuery(settings.Location, queryType);

        var geocoded = await _geocodingService.ResolveLocationAsync(locationQuery);
        if (geocoded == null)
        {
            OutputHandler.PrintError($"Location '{settings.Location}' could not be resolved.");
            return 1;
        }

        var result = _timeConversionService.ConvertToLocalTime(geocoded);
        // Correcting the query name which is lost in geocoding
        result = result with { Query = settings.Location };

        if (settings.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            OutputHandler.PrintJson(result);
        }
        else
        {
            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn();

            grid.AddRow("[bold]Query:[/]", result.Query);
            grid.AddRow("[bold]Resolved As:[/]", result.ResolvedLocation);
            grid.AddRow("[bold]Local Time:[/]", $"[green]{result.LocalTime:yyyy-MM-dd HH:mm:ss}[/]");
            grid.AddRow("[bold]UTC Offset:[/]", result.UtcOffset.ToString());
            grid.AddRow("[bold]Timezone:[/]", result.TimezoneAbbreviation);

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("Location Lookup")
                    .Expand()
                    .Border(BoxBorder.Rounded));
        }

        return 0;
    }
}