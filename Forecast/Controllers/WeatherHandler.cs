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

    public async Task<IEnumerable<CurrentWeather>> GetCurrentWeatherMultiple(
        WeatherProvider provider,
        IEnumerable<LocationDto> locations
    )
    {
        if (!providers.TryGetValue(provider, out var client))
        {
            throw new InvalidOperationException($"Provider {provider} not found");
        }

        var tasks = locations.Select(async location =>
        {
            var temperature = await client.LocationCurrentTemperature(
                location.Lat,
                location.Lon
            );

            return new CurrentWeather(temperature);
        });

        return await Task.WhenAll(tasks);
    }

    public Task<IEnumerable<CurrentWeather>> GetWeatherByCities(
        WeatherProvider provider,
        IEnumerable<City> cities
    )
    {
        throw new NotImplementedException();
    }
}