using System.Net;
using System.Text;
using Forecast.Clients;
using Forecast.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Forecast.Tests.Clients;

public class OpenWeatherForecastClientTests
{
    private readonly IConfiguration configuration;

    public OpenWeatherForecastClientTests()
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

    private OpenWeatherDataClient CreateClient(HttpResponseMessage response)
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
            BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/")
        };

        return new OpenWeatherDataClient(configuration, httpClient);
    }

    // Тест 1: должен вернуть forecast при валидном ответе
    [Fact]
    public async Task Should_Return_Forecast_When_Response_Is_Valid()
    {
        var json = """
        {
            "list": [
                {
                    "dt_txt": "2026-04-30 12:00:00",
                    "main": {
                        "temp_min": 18,
                        "temp_max": 22
                    },
                    "weather": [
                        {
                            "description": "clear sky"
                        }
                    ]
                }
            ]
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var client = CreateClient(response);

        var result = await client.LocationForecast(53.9m, 27.56m);

        Assert.Single(result);
        Assert.Equal("clear sky", result.First().Description);
    }

    // Тест 2: должен выбрасывать ApiCallException при плохом статусе
    [Fact]
    public async Task Should_Throw_ApiCallException_When_Status_Is_Not_Success()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

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
    public async Task Should_Return_Empty_List_When_No_Data()
    {
        var json = """{ "list": [] }""";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var client = CreateClient(response);

        var result = await client.LocationForecast(1m, 1m);

        Assert.Empty(result);
    }
}