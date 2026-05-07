using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TimezoneMeetingCli.Infrastructure;
using TimezoneMeetingCli.Models;
using TimezoneMeetingCli.Services.Geocoding;
using TimezoneMeetingCli.Services.TimeConversion;

namespace TimezoneMeetingCli.Commands;

public class ScheduleCommand : AsyncCommand<ScheduleCommand.Settings>
{
    private readonly IGeocodingService _geocodingService;
    private readonly ITimeConversionService _timeConversionService;

    public ScheduleCommand(IGeocodingService geocodingService, ITimeConversionService timeConversionService)
    {
        _geocodingService = geocodingService;
        _timeConversionService = timeConversionService;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<time>")]
        [Description("The local time to schedule, e.g., \"2026-05-15T14:00\" or just \"14:00\".")]
        public string Time { get; set; } = string.Empty;

        [CommandArgument(1, "<locations>")]
        [Description("A space-separated list of target participant locations.")]
        public string[] Locations { get; set; } = Array.Empty<string>();

        [CommandOption("--format")]
        [Description("Output format, defaults to text. Valid options: text, json.")]
        [DefaultValue("text")]
        public string Format { get; set; } = "text";
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Locations.Length < 1)
        {
            OutputHandler.PrintError("At least one location must be provided.");
            return 1;
        }

        if (!DateTime.TryParse(settings.Time, out var parsedDateTime))
        {
            OutputHandler.PrintError($"Could not parse time: {settings.Time}");
            return 1;
        }

        // Assume the parsed time is in the local system's timezone
        var baseTimeOffset = new DateTimeOffset(parsedDateTime, DateTimeOffset.Now.Offset);
        var results = new List<LocalizedTimeResult>();

        foreach (var locationStr in settings.Locations)
        {
            var isZipCode = int.TryParse(locationStr, out _);
            var queryType = isZipCode ? QueryType.ZipCode : QueryType.CityName;
            var locationQuery = new LocationQuery(locationStr, queryType);

            var geocoded = await _geocodingService.ResolveLocationAsync(locationQuery, cancellationToken);
            if (geocoded == null)
            {
                OutputHandler.PrintError($"Location '{locationStr}' could not be resolved. Skipping...");
                continue;
            }

            var result = _timeConversionService.ConvertToLocalTime(geocoded, baseTimeOffset);
            results.Add(result with { Query = locationStr });
        }

        var proposal = new MeetingProposal(baseTimeOffset, results);

        if (settings.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            OutputHandler.PrintJson(proposal);
        }
        else
        {
            var table = new Table();
            table.AddColumn("Query");
            table.AddColumn("Resolved Location");
            table.AddColumn("Local Time");
            table.AddColumn("Timezone");

            foreach (var res in proposal.ParticipantResults)
            {
                table.AddRow(
                    res.Query,
                    res.ResolvedLocation,
                    $"[green]{res.LocalTime:yyyy-MM-dd HH:mm:ss}[/]",
                    $"{res.TimezoneAbbreviation} (UTC{(res.UtcOffset.TotalHours >= 0 ? "+" : "")}{res.UtcOffset.TotalHours:0.##})"
                );
            }

            AnsiConsole.Write(
                new Panel(table)
                    .Header($"[bold]Meeting Proposed For:[/] {baseTimeOffset:yyyy-MM-dd HH:mm:ss} (Your Local Time)")
                    .Expand()
                    .Border(BoxBorder.Rounded));
        }

        return 0;
    }
}