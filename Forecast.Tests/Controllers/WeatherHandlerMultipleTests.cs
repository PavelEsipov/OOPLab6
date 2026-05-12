using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models.Weather;
using Moq;

namespace Forecast.Tests.Controllers;

public class WeatherHandlerMultipleTests
{
    // Тест 1: должен вернуть weather для нескольких локаций
    [Fact]
    public async Task Should_Return_Weather_For_Multiple_Locations()
    {
        // Arrange
        var clientMock = new Mock<IWeatherDataClient>();

        clientMock
            .Setup(x => x.Provider)
            .Returns(WeatherProvider.OpenWeather);

        clientMock
            .Setup(x =>
                x.LocationCurrentTemperature(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>()
                )
            )
            .ReturnsAsync(20m);

        var handler = new WeatherHandler([
            clientMock.Object
        ]);

        var locations = new List<LocationDto>
        {
            new(53.9m, 27.56m),
            new(51.50m, -0.12m)
        };

        // Act
        var result = await handler.GetCurrentWeatherMultiple(
            WeatherProvider.OpenWeather,
            locations
        );

        // Assert
        Assert.Equal(2, result.Count());
    }

    // Тест 2: должен выбрасывать exception если provider не найден
    [Fact]
    public async Task Should_Throw_When_Provider_Not_Found()
    {
        // Arrange
        var handler = new WeatherHandler([]);

        var locations = new List<LocationDto>
        {
            new(1m, 1m)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.GetCurrentWeatherMultiple(
                WeatherProvider.OpenWeather,
                locations
            )
        );
    }
}