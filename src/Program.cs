// Configure the host builder for both Windows Service and Web API
var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => options.ServiceName = "MinimalApiWindowsService")
    .ConfigureServices(static (hostContext, services) =>
    {
        // Register our ApiService as a hosted service
        services.AddHostedService<ApiService>();

        // Register other services as needed here (if any) 
        // ...
        // e.g. services.AddSingleton<WeatherForecastService>();
    });

// Build and run the host
await builder.Build().RunAsync();