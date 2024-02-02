using System.Text;
using System.Text.Json.Serialization;

namespace westatus;

public class WeatherService : IDisposable
{
    private readonly Config _config;

    private readonly HttpClient _httpClient = new HttpClient();

    private readonly Timer _timer;
    
    private WeatherForecast? _data;

    private int CurrentHour => DateTime.UtcNow.Hour;

    private int ForecastedHour => DateTime.UtcNow.Hour + _config.Weather.ForecastHours;

    public WeatherService(Config config)
    {
        _config = config;

        _timer = new Timer(async x =>
        {
            _data = await _httpClient.GetFromJsonAsync($"https://api.open-meteo.com/v1/forecast?latitude={_config.Weather.Latitude}&longitude={_config.Weather.Longitude}&timezone=GMT&hourly=temperature_2m,precipitation_probability,precipitation,weather_code&forecast_days=2", WeatherSourceGenerationContext.Default.WeatherForecast);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(Math.Max(_config.Weather.SyncFrequency, 100)));
    }

    public string Format
    {
        get
        {
            if (_data is null)
            {
                return "No weather data";
            }

            StringBuilder sb = new StringBuilder(_config.Weather.Format);

            sb.Replace("{{ current_icon }}", GetIconFromCode(_data.Hourly.WeatherCode[CurrentHour]!.Value));
            sb.Replace("{{ current_temp }}", ((int)Math.Round(_data.Hourly.Temperature2m[CurrentHour]!.Value)).ToString());

            sb.Replace("{{ trend_icon }}", Trend);

            sb.Replace("{{ forecast_icon }}", GetIconFromCode(_data.Hourly.WeatherCode[ForecastedHour]!.Value));
            sb.Replace("{{ forecast_temp }}", ((int)Math.Round(_data.Hourly.Temperature2m[ForecastedHour]!.Value)).ToString());

            return sb.ToString();
        }
    }

    private string Trend
    {
        get
        {
            if (_data is null)
            {
                return "";
            }

            double delta = _data.Hourly.Temperature2m[CurrentHour]!.Value - _data.Hourly.Temperature2m[ForecastedHour]!.Value;

            delta = Math.Round(delta);

            if (delta > 0)
            {
                return "\uF30E";
            }
            else if (delta < 0)
            {
                return "\uF310";
            }
            else
            {
                return "\uF30F";
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private static string GetIconFromCode(int weatherCode)
    {
        // codes from https://open-meteo.com/en/docs
        // scroll all the way down
        // why don't these docs use markdown wtf

        return weatherCode switch
        {
            0 => IsDayTime(DateTime.Now) ? "\uF00D" : "\uF02E",
            1 => IsDayTime(DateTime.Now) ? "\uF00C" : "\uF031",
            2 => IsDayTime(DateTime.Now) ? "\uF002" : "\uF031",
            3 => "\uF013",
            45 or 48 => "\uF063",

            51 or 61 or 80 => "\uF017",
            53 or 63 or 81 => "\uF015",
            55 or 65 or 82 => "\uF019",

            56 or 57 or 66 or 67 => "\uF01A",

            71 or 73 or 75 or 77 or 85 or 86 => "\uF01B",

            95 => "\uF01D",
            96 or 99 => "\uF01E",

            _ => "\uF049"
        };
    }

    private static bool IsDayTime(DateTime currentTime)
    {
        TimeSpan dayStart = new TimeSpan(6, 0, 0);
        TimeSpan dayEnd = new TimeSpan(18, 0, 0);

        return currentTime.TimeOfDay >= dayStart && currentTime.TimeOfDay <= dayEnd;
    }

    public record Daily(
        [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
        [property: JsonPropertyName("weather_code")] IReadOnlyList<int?> WeatherCode
    );

    public record DailyUnits(
        [property: JsonPropertyName("time")] string Time,
        [property: JsonPropertyName("weather_code")] string WeatherCode
    );

    public record Hourly(
        [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
        [property: JsonPropertyName("temperature_2m")] IReadOnlyList<double?> Temperature2m,
        [property: JsonPropertyName("precipitation_probability")] IReadOnlyList<int?> PrecipitationProbability,
        [property: JsonPropertyName("precipitation")] IReadOnlyList<double?> Precipitation,
        [property: JsonPropertyName("weather_code")] IReadOnlyList<int?> WeatherCode
    );

    public record HourlyUnits(
        [property: JsonPropertyName("time")] string Time,
        [property: JsonPropertyName("temperature_2m")] string Temperature2m,
        [property: JsonPropertyName("precipitation_probability")] string PrecipitationProbability,
        [property: JsonPropertyName("precipitation")] string Precipitation,
        [property: JsonPropertyName("weather_code")] string WeatherCode
    );

    public record WeatherForecast(
        [property: JsonPropertyName("latitude")] double? Latitude,
        [property: JsonPropertyName("longitude")] double? Longitude,
        [property: JsonPropertyName("generationtime_ms")] double? GenerationtimeMs,
        [property: JsonPropertyName("utc_offset_seconds")] int? UtcOffsetSeconds,
        [property: JsonPropertyName("timezone")] string Timezone,
        [property: JsonPropertyName("timezone_abbreviation")] string TimezoneAbbreviation,
        [property: JsonPropertyName("elevation")] double? Elevation,
        [property: JsonPropertyName("hourly_units")] HourlyUnits HourlyUnits,
        [property: JsonPropertyName("hourly")] Hourly Hourly,
        [property: JsonPropertyName("daily_units")] DailyUnits DailyUnits,
        [property: JsonPropertyName("daily")] Daily Daily
    );
}

// Souce generation based json, rather than reflection
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WeatherService.WeatherForecast))]
[JsonSerializable(typeof(WeatherService.HourlyUnits))]
[JsonSerializable(typeof(WeatherService.Hourly))]
[JsonSerializable(typeof(WeatherService.DailyUnits))]
[JsonSerializable(typeof(WeatherService.Daily))]
[JsonSerializable(typeof(IReadOnlyList<int?>))]
[JsonSerializable(typeof(IReadOnlyList<double?>))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
internal partial class WeatherSourceGenerationContext : JsonSerializerContext
{
}