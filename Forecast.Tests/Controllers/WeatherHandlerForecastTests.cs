using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models;
using Forecast.Models.Weather;
using Moq;

namespace Forecast.Tests.Controllers;

public class WeatherHandlerForecastTests
{
    // Тест 1: должен вернуть forecast от OpenWeather provider
    [Fact]
    public async Task Should_Return_OpenWeather_Forecast()
    {
        // Arrange
        var openWeatherMock = new Mock<IWeatherDataClient>();
        var googleWeatherMock = new Mock<IWeatherDataClient>();

        openWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.OpenWeather);

        googleWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.GoogleWeather);

        var forecast = new List<ForecastWeather>
        {
            new(
                new DateOnly(2026, 5, 1),
                15m,
                21m,
                "clear sky"
            )
        };

        openWeatherMock
            .Setup(x => x.LocationForecast(53.9m, 27.5667m))
            .ReturnsAsync(forecast);

        var providers = new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        };

        var handler = new WeatherHandler(providers);

        // Act
        var result = await handler.GetForecast(
            WeatherProvider.OpenWeather,
            53.9m,
            27.5667m
        );

        // Assert
        Assert.Single(result);

        openWeatherMock.Verify(
            x => x.LocationForecast(53.9m, 27.5667m),
            Times.Once
        );

        googleWeatherMock.Verify(
            x => x.LocationForecast(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    // Тест 2: должен вернуть forecast от GoogleWeather provider
    [Fact]
    public async Task Should_Return_GoogleWeather_Forecast()
    {
        // Arrange
        var openWeatherMock = new Mock<IWeatherDataClient>();
        var googleWeatherMock = new Mock<IWeatherDataClient>();

        openWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.OpenWeather);

        googleWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.GoogleWeather);

        var forecast = new List<ForecastWeather>
        {
            new(
                new DateOnly(2026, 5, 1),
                12m,
                20m,
                "Sunny"
            )
        };

        googleWeatherMock
            .Setup(x => x.LocationForecast(35.6764m, 139.6500m))
            .ReturnsAsync(forecast);

        var providers = new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        };

        var handler = new WeatherHandler(providers);

        // Act
        var result = await handler.GetForecast(
            WeatherProvider.GoogleWeather,
            35.6764m,
            139.6500m
        );

        // Assert
        Assert.Single(result);

        googleWeatherMock.Verify(
            x => x.LocationForecast(35.6764m, 139.6500m),
            Times.Once
        );

        openWeatherMock.Verify(
            x => x.LocationForecast(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    // Тест 3: должен выбрасывать exception если provider не найден
    [Fact]
    public async Task Should_Throw_When_Forecast_Provider_Not_Found()
    {
        // Arrange
        var openWeatherMock = new Mock<IWeatherDataClient>();

        openWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.OpenWeather);

        var providers = new List<IWeatherDataClient>
        {
            openWeatherMock.Object
        };

        var handler = new WeatherHandler(providers);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.GetForecast(
                WeatherProvider.GoogleWeather,
                53.9m,
                27.5667m
            )
        );
    }
}