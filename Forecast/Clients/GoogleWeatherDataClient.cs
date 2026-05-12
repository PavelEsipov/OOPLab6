using Forecast.Utils;

namespace Forecast.Clients;

public class GoogleWeatherDataClient : IWeatherDataClient
{
    public Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        // TODO: реализовать реальный вызов к API Google Weather
        return Task.FromResult(25.0m);
    }
}