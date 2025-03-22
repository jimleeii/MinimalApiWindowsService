namespace MinimalApiWindowsService.Models;

/// <summary>
/// A record representing a weather forecast.
/// </summary>
/// <param name="Date">The date of the forecast.</param>
/// <param name="TemperatureC">The temperature in degrees Celsius.</param>
/// <param name="Summary">A brief summary of the forecast.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// The temperature in degrees Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
