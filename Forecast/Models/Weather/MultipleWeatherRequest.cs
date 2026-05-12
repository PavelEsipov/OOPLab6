namespace Forecast.Models.Weather;

public record MultipleWeatherRequest(
    WeatherProvider Provider,
    List<LocationDto> Locations
);