using Tomlyn;
using Tomlyn.Model;
using westatus;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfigurationRoot commandLine = builder.Configuration
    .AddCommandLine(args, new Dictionary<string, string>() { { "-p", "config-path" }, { "--path", "config-path" } })
    .Build();

string path = commandLine.GetValue<string>("config-path") ?? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/westatus/config.toml";

Config config = new Config();

if (File.Exists(path))
{
    config = Toml.ToModel<Config>(File.ReadAllText(path));
}

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