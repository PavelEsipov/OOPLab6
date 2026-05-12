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
            var url =
                $"v1/currentConditions:lookup" +
                $"?key={apiKey}" +
                $"&location.latitude={latitude}" +
                $"&location.longitude={longitude}";

            var response = await client.GetAsync(url);

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

    public Task<IEnumerable<ForecastWeather>> LocationForecast(decimal latitude,decimal longitude)
    {
        throw new NotImplementedException();
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