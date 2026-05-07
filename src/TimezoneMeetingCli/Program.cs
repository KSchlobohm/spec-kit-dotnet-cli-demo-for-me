using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using TimezoneMeetingCli.Infrastructure;
using TimezoneMeetingCli.Services.Geocoding;
using TimezoneMeetingCli.Services.TimeConversion;

var services = new ServiceCollection();
services.AddHttpClient<IGeocodingService, OpenMeteoGeocodingService>();
services.AddSingleton<ITimeConversionService, NodaTimeConversionService>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddBranch("time", time =>
    {
        time.AddCommand<TimezoneMeetingCli.Commands.LookupCommand>("lookup");
    });
});

return app.Run(args);
