using Forecast.Models.Weather;

namespace Forecast.Controllers;

class CurrentWeatherController(WeatherHandler handler)
{
    private readonly WeatherHandler handler = handler;

    public Task<CurrentWeather> GetCurrentWeather(
        WeatherProvider provider,
        decimal latitude,
        decimal longitude
    )
    {
        return handler.GetCurrentWeather(provider, latitude, longitude);
    }

    public Task<IEnumerable<ForecastWeather>> GetForecast(
        WeatherProvider provider,
        decimal latitude,
        decimal longitude
    )
    {
        return handler.GetForecast(provider, latitude, longitude);
    }
}