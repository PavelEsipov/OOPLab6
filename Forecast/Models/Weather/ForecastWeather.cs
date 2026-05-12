namespace Forecast.Models.Weather;

public record ForecastWeather(
    DateOnly Date,
    decimal MinTemperature,
    decimal MaxTemperature,
    string Description
);