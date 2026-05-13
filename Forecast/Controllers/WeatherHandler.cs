using Forecast.Clients;
using Forecast.Models.Weather;

namespace Forecast.Controllers;

public class WeatherHandler(IEnumerable<IWeatherDataClient> clients)
{
    private readonly Dictionary<WeatherProvider, IWeatherDataClient> providers =
        clients.ToDictionary(x => x.Provider, x => x);

    private static readonly Dictionary<City, (decimal lat, decimal lon)> cityMap = new()
    {
        { City.Minsk, (53.9m, 27.5667m) },
        { City.London, (51.5074m, -0.1278m) },
        { City.Tokyo, (35.6895m, 139.6917m) },
        { City.Shanghai, (31.2304m, 121.4737m) },
        { City.Warsaw, (52.2297m, 21.0122m) }
    };

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

    public async Task<CurrentWeather> GetWeatherByCity(
        WeatherProvider provider,
        City city
    )
    {
        if (!providers.TryGetValue(provider, out var client))
        {
            throw new InvalidOperationException($"Provider {provider} not found");
        }

        if (!cityMap.TryGetValue(city, out var coords))
        {
            throw new InvalidOperationException($"City {city} not supported");
        }

        var temp = await client.LocationCurrentTemperature(
            coords.lat,
            coords.lon
        );

        return new CurrentWeather(temp);
    }
}