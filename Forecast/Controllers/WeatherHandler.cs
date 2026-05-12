using Forecast.Clients;
using Forecast.Models;
using Forecast.Models.Weather;

namespace Forecast.Controllers;

public class WeatherHandler(IEnumerable<IWeatherDataClient> providers)
{
    public Task<CurrentWeather> GetCurrentWeather(
        WeatherProvider provider,
        decimal latitude,
        decimal longitude
    )
    {
        // TODO: реализовать метод, который будет использовать нужного провайдера для получения данных о погоде
        throw new NotImplementedException();
    }
}