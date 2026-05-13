using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models.Weather;
using Moq;

namespace Forecast.Tests.Controllers;

public class CityWeatherHandlerTests
{
    // Тест 1: должен вернуть weather для города через OpenWeather
    [Fact]
    public async Task Should_Return_OpenWeather_For_City()
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
            .Setup(x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ReturnsAsync(10m);

        var handler = new WeatherHandler(new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        });

        // Act
        var result = await handler.GetWeatherByCity(
            WeatherProvider.OpenWeather,
            City.Minsk
        );

        // Assert
        Assert.NotNull(result);

        openWeatherMock.Verify(
            x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Once
        );

        googleWeatherMock.Verify(
            x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    // Тест 2: должен вернуть weather для GoogleWeather
    [Fact]
    public async Task Should_Return_GoogleWeather_For_City()
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
            .Setup(x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ReturnsAsync(20m);

        var handler = new WeatherHandler(new List<IWeatherDataClient>
        {
            openWeatherMock.Object,
            googleWeatherMock.Object
        });

        // Act
        var result = await handler.GetWeatherByCity(
            WeatherProvider.GoogleWeather,
            City.Tokyo
        );

        // Assert
        Assert.NotNull(result);

        googleWeatherMock.Verify(
            x => x.LocationCurrentTemperature(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Once
        );
    }

    // Тест 3: неизвестный provider → exception
    [Fact]
    public async Task Should_Throw_When_Provider_Not_Found()
    {
        // Arrange
        var openWeatherMock = new Mock<IWeatherDataClient>();

        openWeatherMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.OpenWeather);

        var handler = new WeatherHandler(new List<IWeatherDataClient>
        {
            openWeatherMock.Object
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.GetWeatherByCity(
                WeatherProvider.GoogleWeather,
                City.Minsk
            )
        );
    }
}