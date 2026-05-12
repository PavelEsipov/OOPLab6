using System.Net;
using System.Text;
using Forecast.Clients;
using Forecast.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Forecast.Tests.Clients;

public class GoogleWeatherDataClientTests
{
    private readonly IConfiguration configuration;

    public GoogleWeatherDataClientTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "GOOGLE_WEATHER_BASE_URL", "https://api" },
            { "GOOGLE_WEATHER_API_KEY", "test-key" }
        };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    // Тест 1: должен возвращать температуру при корректном ответе Google Weather API
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
                            "temperature": 30.5
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new GoogleWeatherDataClient();

        // Act
        var result = await client.LocationCurrentTemperature(10m, 20m);

        // Assert
        Assert.Equal(30.5m, result);
    }

    // Тест 2: должен выбрасывать ApiCallException при ошибочном статусе ответа
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Status_Is_NotSuccess()
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

        var client = new GoogleWeatherDataClient();

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(10m, 20m)
        );
    }

    // Тест 3: должен выбрасывать ApiCallException при битом JSON
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
                        "{ invalid json ",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new GoogleWeatherDataClient();

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(10m, 20m)
        );
    }

    // Тест 4: должен выбрасывать ApiCallException при HttpRequestException
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Http_Fails()
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
            .ThrowsAsync(new HttpRequestException("network error"));

        var httpClient = new HttpClient(handlerMock.Object);

        var client = new GoogleWeatherDataClient();

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(10m, 20m)
        );
    }

    // Тест 5: должен выбрасывать ApiCallException если поле temperature отсутствует (null структура)
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Temperature_Is_Missing()
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
                            "temperature": null
                        }
                        """,
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new GoogleWeatherDataClient();

        // Act & Assert
        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(10m, 20m)
        );
    }
}