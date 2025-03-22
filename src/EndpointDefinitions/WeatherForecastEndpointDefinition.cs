using EndpointDefinition;

namespace MinimalApiWindowsService.EndpointDefinitions;

/// <summary>
/// The weather forecast endpoint definition.
/// </summary>
public class WeatherForecastEndpointDefinition : IEndpointDefinition
{
    /// <summary>
    /// Defines the endpoints.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <param name="env">The env.</param>
    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        app.MapGet("WeatherForecast", (WeatherForecastService weatherService) => weatherService.GetForecast());
    }

    /// <summary>
    /// Defines the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton(new WeatherForecastService());
    }
}