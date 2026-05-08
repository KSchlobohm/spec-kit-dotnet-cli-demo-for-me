using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Text.Json;
using TimezoneMeetingCli.Infrastructure;
using TimezoneMeetingCli.Models;
using TimezoneMeetingCli.Services.Geocoding;
using TimezoneMeetingCli.Services.TimeConversion;

var services = new ServiceCollection();
var geocodingFixtureJson = Environment.GetEnvironmentVariable("TIMEZONE_MEETING_CLI_GEOCODING_FIXTURE_JSON");

if (string.IsNullOrWhiteSpace(geocodingFixtureJson))
{
    services.AddHttpClient<IGeocodingService, OpenMeteoGeocodingService>();
}
else
{
    var geocodingFixture = JsonSerializer.Deserialize<Dictionary<string, GeocodedLocation>>(geocodingFixtureJson)
        ?? new Dictionary<string, GeocodedLocation>();
    services.AddSingleton<IGeocodingService>(new FixtureGeocodingService(
        new Dictionary<string, GeocodedLocation>(geocodingFixture, StringComparer.OrdinalIgnoreCase)));
}

services.AddSingleton<ITimeConversionService, NodaTimeConversionService>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddBranch("time", time =>
    {
        time.AddCommand<TimezoneMeetingCli.Commands.LookupCommand>("lookup");
        time.AddCommand<TimezoneMeetingCli.Commands.ScheduleCommand>("schedule");
    });
});

return app.Run(args);
