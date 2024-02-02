using CommandLine;
using Tomlyn;
using westatus;

Options options = Parser.Default.ParseArguments<Options>(args).Value;

Config config = File.Exists(options.ConfigPath) ? Toml.ToModel<Config>(File.ReadAllText(options.ConfigPath)) : new Config();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif
builder.Services.AddSingleton(config);

if (config.Weather.Enabled)
{
    builder.Services.AddSingleton<WeatherService>();
}

WebApplication app = builder.Build();

app.MapGet("/", () => "hi particles");

WeatherService? weather = app.Services.GetService<WeatherService>();
app.MapGet("/weather", () => weather?.Format ?? "Weather service not enabled");

app.Run($"http://localhost:{config.Network.Port}");

public class Options
{
    [Option('p', "--path", Required = false, HelpText = "Path to TOML config file; defaults to ~/.config/westatus/config.toml")]
    public string ConfigPath { get; set; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/westatus/config.toml";
}