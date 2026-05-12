using System.Net;
using System.Text;
using Forecast.Clients;
using Forecast.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Forecast.Tests.Clients;

public class GoogleWeatherForecastClientTests
{
    private readonly IConfiguration configuration;

    public GoogleWeatherForecastClientTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "GOOGLE_WEATHER_BASE_URL", "https://weather.googleapis.com/" },
            { "GOOGLE_WEATHER_API_KEY", "test-api-key" }
        };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private GoogleWeatherDataClient CreateClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://weather.googleapis.com/")
        };

        return new GoogleWeatherDataClient(configuration, httpClient);
    }

    // Тест 1: должен вернуть forecast при валидном ответе
    [Fact]
    public async Task Should_Return_Forecast_When_Response_Is_Valid()
    {
        var json = """
        {
            "forecastDays": [
                {
                    "displayDate": {
                        "year": 2026,
                        "month": 4,
                        "day": 30
                    },
                    "maxTemperature": {
                        "degrees": 20
                    },
                    "minTemperature": {
                        "degrees": 10
                    },
                    "daytimeForecast": {
                        "weatherCondition": {
                            "description": {
                                "text": "Sunny"
                            }
                        }
                    }
                }
            ]
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var client = CreateClient(response);

        var result = await client.LocationForecast(1m, 1m);

        Assert.Single(result);
        Assert.Equal("Sunny", result.First().Description);
    }

    // Тест 2: должен выбрасывать ApiCallException при плохом статусе
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Status_Is_Not_Success()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var client = CreateClient(response);

        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationForecast(1m, 1m)
        );
    }

    // Тест 3: должен выбрасывать ApiCallException при битом JSON
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Response_Is_Invalid()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("INVALID_JSON")
        };

        var client = CreateClient(response);

        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationForecast(1m, 1m)
        );
    }

    // Тест 4: должен вернуть пустой список
    [Fact]
    public async Task Should_Return_Empty_List_When_Response_Is_Empty()
    {
        var json = """{ "forecastDays": [] }""";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var client = CreateClient(response);

        var result = await client.LocationForecast(1m, 1m);

        Assert.Empty(result);
    }
}