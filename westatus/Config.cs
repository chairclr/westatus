namespace westatus;

public class Config
{
    public NetworkTable Network { get; set; } = new NetworkTable();

    public WeatherTable Weather { get; set; } = new WeatherTable();

    // manual toml configuration that supports naot
    // not using rn since it's just not worth it
    /*
    public void LoadFromTable(TomlTable toml)
    {
        object value;

        if (toml.TryGetValue("network", out value))
        {
            TomlTable? networkToml = value as TomlTable;

            if (networkToml is not null)
            {
                if (networkToml.TryGetValue("port", out value))
                    Network.Port = value as ushort? ?? Network.Port;
            }
        }

        if (toml.TryGetValue("weather", out value))
        {
            TomlTable? weatherToml = value as TomlTable;

            if (weatherToml is not null)
            {
                if (weatherToml.TryGetValue("enabled", out value))
                    Weather.Enabled = value as bool? ?? Weather.Enabled;

                if (weatherToml.TryGetValue("sync_frequency", out value))
                    Weather.SyncFrequency = value as int? ?? Weather.SyncFrequency;

                if (weatherToml.TryGetValue("latitude", out value))
                    Weather.Latitude = value as double? ?? Weather.Latitude;

                if (weatherToml.TryGetValue("longitude", out value))
                    Weather.Longitude = value as double? ?? Weather.Longitude;

                if (weatherToml.TryGetValue("forecast_hours", out value))
                    Weather.ForecastHours = value as int? ?? Weather.ForecastHours;

                if (weatherToml.TryGetValue("format", out value))
                    Weather.Format = value as string ?? Weather.Format;
            }
        }
    }*/

    public class NetworkTable
    {
        public ushort Port { get; set; } = 40001;
    }

    public class WeatherTable
    {
        public bool Enabled { get; set; } = false;

        public int SyncFrequency { get; set; } = 1800;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int ForecastHours { get; set; } = 3;

        public string Format { get; set; } = "";
    }
}