
namespace westatus;

public class Config
{
    public NetworkTable Network { get; set; } = new NetworkTable();

    public WeatherTable Weather { get; set; } = new WeatherTable();

    public class NetworkTable
    {
        public ushort Port { get; set; } = 40001;
    }

    public class WeatherTable
    {
        public bool Enabled { get; set; } = false;

        public int SyncFrequency { get; set; } = 400;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int ForecastHours { get; set; } = 3;

        public string Format { get; set; } = "";
    }
}