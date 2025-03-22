namespace MinimalApiWindowsService.Services;

/// <summary>
/// A service that returns a sequence of <see cref="WeatherForecast"/> objects, representing
/// a 5-day weather forecast.
/// </summary>
public class WeatherForecastService
{
    // Pre-generated list of weather summaries
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    // Random number generator
    private readonly Random _random = Random.Shared;

    /// <summary>
    /// Returns 5 days of weather forecast.
    /// </summary>
    /// <returns>A sequence of 5 <see cref="WeatherForecast"/> objects.</returns>
    public IEnumerable<WeatherForecast> GetForecast()
    {
        // Generate random weather forecasts
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            _random.Next(-20, 55),
            Summaries[_random.Next(Summaries.Length)]));

        return forecast;
    }
}