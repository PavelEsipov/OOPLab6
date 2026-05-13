using System.ComponentModel;
using Forecast.Controllers;
using Forecast.Models.Weather;
using Forecast.Shared.Responses;
using Forecast.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Forecast.Api;

public static class WeatherApi
{
    public static RouteGroupBuilder MapCurrentWeatherApi(this RouteGroupBuilder groups)
    {
        groups
            .MapGet("weather", WeatherApi.HandleGetCurrentWeather)
            .WithName("GetCurrentWeather")
            .WithDisplayName("Get Current Weather")
            .WithTags(["weather"])
            .WithDescription("Returns current weather for given coordinates");

        return groups;
    }

    public static RouteGroupBuilder MapForecastApi(this RouteGroupBuilder groups)
    {
        groups
            .MapGet("forecast", WeatherApi.HandleGetForecast)
            .WithName("GetForecast")
            .WithDisplayName("Get Forecast")
            .WithTags(["weather"])
            .WithDescription("Returns weather forecast for given coordinates");

        return groups;
    }

    public static RouteGroupBuilder MapMultipleCurrentWeatherApi(this RouteGroupBuilder groups)
    {
        groups
            .MapPost(
                "weather/multiple",
                WeatherApi.HandleGetCurrentWeatherMultiple
            )
            .WithName("GetCurrentWeatherMultiple")
            .WithDisplayName("Get Current Weather Multiple")
            .WithTags(["weather"])
            .WithDescription(
                "Returns current weather for multiple locations"
            );

        return groups;
    }

    public static RouteGroupBuilder MapCityWeatherApi(this RouteGroupBuilder groups)
    {
        groups
            .MapGet("weather/by-city", HandleGetWeatherByCity)
            .WithName("GetWeatherByCity")
            .WithTags(["weather"])
            .WithDescription("Returns weather for a single city");

        return groups;
    }

    private static async Task<
        Results<Ok<Success<CurrentWeather>>, BadRequest<Status>, InternalServerError<Status>>
    > HandleGetCurrentWeather(
        [FromServices] CurrentWeatherController controller,
        [FromQuery] WeatherProvider provider,
        [DefaultValue("18.300231990440125")] string lat,
        [DefaultValue("-64.8251590359234")] string lon
    )
    {
        try
        {
            var latitude = decimal.Parse(lat);
            var longitude = decimal.Parse(lon);

            var weather = await controller.GetCurrentWeather(provider, latitude, longitude);

            return TypedResults.Ok(Success.Create(200, "success", weather));
        }
        catch (FormatException)
        {
            return TypedResults.BadRequest(Status.Create(400, "invalid coordinates"));
        }
        catch (OverflowException)
        {
            return TypedResults.BadRequest(Status.Create(400, "invalid coordinates"));
        }
        catch (ApiCallException e)
        {
            return TypedResults.InternalServerError(Status.Create(500, e.Message));
        }
    }

    private static async Task<
    Results<Ok<Success<IEnumerable<ForecastWeather>>>, BadRequest<Status>, InternalServerError<Status>>
    > HandleGetForecast(
        [FromServices] CurrentWeatherController controller,
        [FromQuery] WeatherProvider provider,
        [DefaultValue("18.300231990440125")] string lat,
        [DefaultValue("-64.8251590359234")] string lon
    )
    {
        try
        {
            var latitude = decimal.Parse(lat);
            var longitude = decimal.Parse(lon);

            var forecast = await controller.GetForecast(provider, latitude, longitude);

            return TypedResults.Ok(Success.Create(200, "success", forecast));
        }
        catch (FormatException)
        {
            return TypedResults.BadRequest(Status.Create(400, "invalid coordinates"));
        }
        catch (OverflowException)
        {
            return TypedResults.BadRequest(Status.Create(400, "invalid coordinates"));
        }
        catch (ApiCallException e)
        {
            return TypedResults.InternalServerError(Status.Create(500, e.Message));
        }
    }

    private static async Task<
    Results<Ok<Success<IEnumerable<CurrentWeather>>>,BadRequest<Status>,InternalServerError<Status>>
    > HandleGetCurrentWeatherMultiple(
        [FromServices] CurrentWeatherController controller,
        [FromBody] MultipleWeatherRequest request
    )
    {
        try
        {
            if (request.Locations.Count == 0)
            {
                return TypedResults.BadRequest(
                    Status.Create(400, "locations cannot be empty")
                );
            }

            var result = await controller.GetCurrentWeatherMultiple(
                request.Provider,
                request.Locations
            );

            return TypedResults.Ok(
                Success.Create(200, "success", result)
            );
        }
        catch (ApiCallException e)
        {
            return TypedResults.InternalServerError(
                Status.Create(500, e.Message)
            );
        }
    }

    private static async Task<
    Results<Ok<Success<CurrentWeather>>, BadRequest<Status>, InternalServerError<Status>>
    > HandleGetWeatherByCity(
        [FromServices] WeatherHandler handler,
        [FromQuery] WeatherProvider provider,
        [FromQuery] City city
    )
    {
        try
        {
            var result = await handler.GetWeatherByCity(provider, city);

            return TypedResults.Ok(
                Success.Create(200, "success", result)
            );
        }
        catch (ApiCallException e)
        {
            return TypedResults.InternalServerError(
                Status.Create(500, e.Message)
            );
        }
        catch (InvalidOperationException e)
        {
            return TypedResults.BadRequest(
                Status.Create(400, e.Message)
            );
        }
    }
}
