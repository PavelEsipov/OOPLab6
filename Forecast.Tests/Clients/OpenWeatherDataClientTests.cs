using System.Net;
using System.Text;
using Forecast.Clients;
using Forecast.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Forecast.Tests.Clients;

public class OpenWeatherDataClientTests
{
    private readonly IConfiguration configuration;

    public OpenWeatherDataClientTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "OPENWEATHER_BASE_URL", "https://api" },
            { "OPENWEATHER_API_KEY", "test-api-key" }
        };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    // Тест 1: должен возвращать температуру при корректном ответе API
    [Fact]
    public async Task Should_Return_Temperature_When_Response_Is_Valid()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        {
                            "main": {
                                "temp": 21.5
                            }
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new OpenWeatherDataClient(configuration, httpClient);

        // Act
        var result = await client.LocationCurrentTemperature(53.9m, 27.5667m);

        // Assert
        Assert.Equal(21.5m, result);
    }

    // Тест 2: должен выбрасывать ApiCallException при ошибочном статусе ответа
    [Fact]
    public async Task Should_Throw_ApiCallException_When_StatusCode_Is_NotSuccess()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new OpenWeatherDataClient(configuration, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5667m)
        );
    }

    // Тест 3: должен выбрасывать ApiCallException при некорректном JSON
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Response_Is_Invalid()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        {
                            "invalid": "data"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new OpenWeatherDataClient(configuration, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5667m)
        );
    }

    // Тест 4: должен выбрасывать ApiCallException при сетевой ошибке
    [Fact]
    public async Task Should_Throw_ApiCallException_When_HttpRequest_Fails()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new OpenWeatherDataClient(configuration, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5667m)
        );
    }

    // Тест 5: должен выбрасывать ApiCallException если поле main отсутствует (null) в ответе
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Main_Is_Null()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                    {
                        "main": null
                    }
                    """,
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new OpenWeatherDataClient(configuration, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5667m)
        );
    }
}