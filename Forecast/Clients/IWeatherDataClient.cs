using Forecast.Models.Weather;

namespace Forecast.Clients;

public interface IWeatherDataClient
{
    public WeatherProvider Provider { get; }

    Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude);
}
