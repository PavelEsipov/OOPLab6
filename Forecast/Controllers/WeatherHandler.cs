using Forecast.Clients;
using Forecast.Models.Weather;

namespace Forecast.Controllers;

public class WeatherHandler(IEnumerable<IWeatherDataClient> clients)
{
    private readonly Dictionary<WeatherProvider, IWeatherDataClient> providers =
        clients.ToDictionary(x => x.Provider, x => x);

    public async Task<CurrentWeather> GetCurrentWeather(
        WeatherProvider provider,
        decimal latitude,
        decimal longitude
    )
    {
        if (!providers.TryGetValue(provider, out var client))
        {
            throw new InvalidOperationException($"Provider {provider} not found");
        }

        var temperature = await client.LocationCurrentTemperature(latitude, longitude);

        return new CurrentWeather(temperature);
    }

    public async Task<IEnumerable<ForecastWeather>> GetForecast(
        WeatherProvider provider,
        decimal latitude,
        decimal longitude
    )
    {
        if (!providers.TryGetValue(provider, out var client))
        {
            throw new InvalidOperationException($"Provider {provider} not found");
        }

        return await client.LocationForecast(latitude, longitude);
    }

    public Task<IEnumerable<CurrentWeather>> GetCurrentWeatherMultiple(
        WeatherProvider provider,
        IEnumerable<LocationDto> locations
    )
    {
        throw new NotImplementedException();
    }
}