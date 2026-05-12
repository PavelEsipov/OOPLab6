using System.Text.Json;
using System.Text.Json.Serialization;
using Forecast.Utils;
using Forecast.Models.Weather;

namespace Forecast.Clients;

public class OpenWeatherDataClient : IWeatherDataClient
{
    public WeatherProvider Provider => WeatherProvider.OpenWeather;

    private readonly HttpClient client;
    private readonly string apiKey;

    public OpenWeatherDataClient(IConfiguration config, HttpClient httpClient)
    {
        client = httpClient;
        client.BaseAddress = new Uri(config.GetValue<string>("OPENWEATHER_BASE_URL") ?? "");
        apiKey = config.GetValue<string>("OPENWEATHER_API_KEY") ?? "";
    }

    public async Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        try
        {
            var response = await client.GetAsync(
                $"weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException(
                    $"openweather returned bad status: {(ushort)response.StatusCode}"
                );
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();

            return data?.Main?.Temp ?? throw new ApiCallException($"failed to decode response");
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call openweather: {e.Message}.", inner: e);
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
                $"forecast?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException(
                    $"openweather returned bad status: {(ushort)response.StatusCode}"
                );
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherForecastResponse>();

            if (data?.List == null)
                return [];

            var grouped = data.List
                .GroupBy(x => DateTime.Parse(x.DtTxt).Date)
                .Select(g =>
                {
                    var min = g.Min(x => x.Main.TempMin);
                    var max = g.Max(x => x.Main.TempMax);
                    var description = g.First().Weather.FirstOrDefault()?.Description ?? "";

                    return new ForecastWeather(
                        DateOnly.FromDateTime(g.Key),
                        min,
                        max,
                        description
                    );
                })
                .Take(5);

            return grouped;
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call openweather: {e.Message}", e);
        }
        catch (JsonException e)
        {
            throw new ApiCallException($"invalid openweather response", e);
        }
    }
}

class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public required Nested Main { get; set; }

    public class Nested
    {
        [JsonPropertyName("temp")]
        public decimal Temp { get; set; }
    }
}

class OpenWeatherForecastResponse
{
    [JsonPropertyName("list")]
    public List<Item> List { get; set; } = [];

    public class Item
    {
        [JsonPropertyName("dt_txt")]
        public string DtTxt { get; set; } = "";

        [JsonPropertyName("main")]
        public MainData Main { get; set; } = new();

        [JsonPropertyName("weather")]
        public List<WeatherData> Weather { get; set; } = [];
    }

    public class MainData
    {
        [JsonPropertyName("temp_min")]
        public decimal TempMin { get; set; }

        [JsonPropertyName("temp_max")]
        public decimal TempMax { get; set; }
    }

    public class WeatherData
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }
}
