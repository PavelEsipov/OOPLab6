using System.Text.Json;
using System.Text.Json.Serialization;
using Forecast.Utils;
using Forecast.Models.Weather;

namespace Forecast.Clients;

public class GoogleWeatherDataClient : IWeatherDataClient
{
    public WeatherProvider Provider => WeatherProvider.GoogleWeather;

    private readonly HttpClient client;
    private readonly string apiKey;

    public GoogleWeatherDataClient(IConfiguration config, HttpClient httpClient)
    {
        client = httpClient;
        client.BaseAddress = new Uri(config.GetValue<string>("GOOGLE_WEATHER_BASE_URL") ?? "");
        apiKey = config.GetValue<string>("GOOGLE_WEATHER_API_KEY") ?? "";
    }

    public async Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        try
        {
            var response = await client.GetAsync(
                $"v1/currentConditions:lookup?key={apiKey}&location.latitude={latitude}&location.longitude={longitude}"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException(
                    $"google weather returned bad status: {(ushort)response.StatusCode}"
                );
            }

            var data = await response.Content.ReadFromJsonAsync<GoogleWeatherResponse>();

            return data?.Temperature?.Degrees
                ?? throw new ApiCallException("failed to decode response");
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call google weather: {e.Message}.", e);
        }
        catch (JsonException e)
        {
            throw new ApiCallException("failed to decode response", e);
        }
    }

    public async Task<IEnumerable<ForecastWeather>> LocationForecast(
        decimal latitude,
        decimal longitude
    )
    {
        try
        {
            var response = await client.GetAsync(
                $"v1/forecast/days:lookup?key={apiKey}&location.latitude={latitude}&location.longitude={longitude}"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException(
                    $"google weather returned bad status: {(ushort)response.StatusCode}"
                );
            }

            var data = await response.Content.ReadFromJsonAsync<GoogleForecastResponse>();

            if (data?.ForecastDays == null || data.ForecastDays.Count == 0)
                return [];

            return data.ForecastDays
                .Take(5)
                .Select(x =>
                {
                    var date = new DateOnly(
                        x.DisplayDate.Year,
                        x.DisplayDate.Month,
                        x.DisplayDate.Day
                    );

                    return new ForecastWeather(
                        date,
                        x.MinTemperature.Degrees,
                        x.MaxTemperature.Degrees,
                        x.DaytimeForecast.WeatherCondition.Description.Text
                    );
                });
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call google weather: {e.Message}", e);
        }
        catch (JsonException e)
        {
            throw new ApiCallException($"invalid google weather response", e);
        }
    }
}

class GoogleWeatherResponse
{
    [JsonPropertyName("temperature")]
    public required Nested Temperature { get; set; }

    public class Nested
    {
        [JsonPropertyName("degrees")]
        public decimal Degrees { get; set; }
    }
}

class GoogleForecastResponse
{
    [JsonPropertyName("forecastDays")]
    public List<ForecastDay> ForecastDays { get; set; } = [];

    public class ForecastDay
    {
        [JsonPropertyName("displayDate")]
        public DisplayDate DisplayDate { get; set; } = new();

        [JsonPropertyName("daytimeForecast")]
        public ForecastPart DaytimeForecast { get; set; } = new();

        [JsonPropertyName("maxTemperature")]
        public Temperature MaxTemperature { get; set; } = new();

        [JsonPropertyName("minTemperature")]
        public Temperature MinTemperature { get; set; } = new();
    }

    public class DisplayDate
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }
    }

    public class ForecastPart
    {
        [JsonPropertyName("weatherCondition")]
        public WeatherCondition WeatherCondition { get; set; } = new();
    }

    public class WeatherCondition
    {
        [JsonPropertyName("description")]
        public Description Description { get; set; } = new();
    }

    public class Description
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }

    public class Temperature
    {
        [JsonPropertyName("degrees")]
        public decimal Degrees { get; set; }
    }
}