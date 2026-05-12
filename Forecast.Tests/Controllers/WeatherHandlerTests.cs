using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models.Weather;
using Moq;

namespace Forecast.Tests.Controllers;

public class WeatherHandlerTests
{
    // Тест 1: должен использовать OpenWeather provider
    [Fact]
    public async Task Should_Use_OpenWeather_Provider()
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

        openWeatherMock
            .Setup(x => x.LocationCurrentTemperature(53.9m, 27.5667m))
            .ReturnsAsync(15.5m);

        var providers = new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        };

        var handler = new WeatherHandler(providers);

        // Act
        var result = await handler.GetCurrentWeather(
            WeatherProvider.OpenWeather,
            53.9m,
            27.5667m
        );

        // Assert
        Assert.Equal(15.5m, result.Temperature);

        openWeatherMock.Verify(
            x => x.LocationCurrentTemperature(53.9m, 27.5667m),
            Times.Once
        );

        googleWeatherMock.Verify(
            x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    // Тест 2: должен использовать GoogleWeather provider
    [Fact]
    public async Task Should_Use_GoogleWeather_Provider()
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

        googleWeatherMock
            .Setup(x => x.LocationCurrentTemperature(35.6764m, 139.6500m))
            .ReturnsAsync(24.3m);

        var providers = new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        };

        var handler = new WeatherHandler(providers);

        // Act
        var result = await handler.GetCurrentWeather(
            WeatherProvider.GoogleWeather,
            35.6764m,
            139.6500m
        );

        // Assert
        Assert.Equal(24.3m, result.Temperature);

        googleWeatherMock.Verify(
            x => x.LocationCurrentTemperature(35.6764m, 139.6500m),
            Times.Once
        );

        openWeatherMock.Verify(
            x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    // Тест 3: должен выбрасывать InvalidOperationException если provider не найден
    [Fact]
    public async Task Should_Throw_When_Provider_Not_Found()
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
            handler.GetCurrentWeather(
                WeatherProvider.GoogleWeather,
                53.9m,
                27.5667m
            )
        );
    }
}