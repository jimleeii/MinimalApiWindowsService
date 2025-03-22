using EndpointDefinition;
using MinimalApiWindowsService.Extensions;

namespace MinimalApiWindowsService.Services;

/// <summary>
/// Initializes a new instance of the <see cref="ApiService"/> class with the specified logger.
/// </summary>
/// <param name="logger">The logger used to log information, warnings, and errors for the API service.</param>
public class ApiService(ILogger<ApiService> logger) : IHostedService, IDisposable
{
    // Service logger
    private readonly ILogger<ApiService> _logger = logger;

    // Web host
    private IHost? _webHost;

    // Web application cancellation token source
    private CancellationTokenSource? _webAppCts;

    /// <summary>
    /// Starts the API Service. This includes configuring and starting the web application.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API Service is starting...");

        // Create a CancellationTokenSource we can use to stop the web host
        _webAppCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Configure and start the web application
        _webHost = CreateWebApplication();

        try
        {
            await _webHost.RunAsync(_webAppCts.Token);

            _logger.LogInformation("API Service started successfully. API is now accessible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the API Service.");
            throw;
        }
    }

    /// <summary>
    /// Stops the API Service. This includes stopping the web host and triggering
    /// cancellation to stop the web application.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API Service is stopping...");

        // Trigger cancellation to stop the web application
        _webAppCts?.Cancel();

        // Wait for the web host to stop
        if (_webHost != null)
        {
            await _webHost.StopAsync(cancellationToken);
        }

        _logger.LogInformation("API Service stopped. API is no longer accessible.");
    }

    /// <summary>
    /// Creates and configures the web application.
    /// </summary>
    /// <remarks>
    /// This method reads the plugin path from the configuration and adds the plugins,
    /// NuGet plugins, and endpoint definitions to the web application.
    /// It then configures the web application to listen on a specific port (7145)
    /// and defines two endpoints: "/" and "/health".
    /// </remarks>
    /// <returns>The configured web application.</returns>
    private static WebApplication CreateWebApplication()
    {
        var builder = WebApplication.CreateBuilder();

        // Read the plugin path from the configuration
        string pluginPath = builder.Configuration["PluginPath"] ?? throw new InvalidOperationException("PluginPath configuration is missing.");

        // Add plugins
        // ...
        // e.g. builder.Services.AddApiPlugins(pluginPath);

        // Add NuGet plugins
        builder.Services.AddApiNugetPlugins(pluginPath);

        // Add the endpoint definitions
        builder.Services.AddEndpointDefinitions(typeof(IEndpointDefinition));

        // Configure Kestrel to listen on a specific port
        string protocol = builder.Configuration["Protocol"] ?? "https";
        string domain = builder.Configuration["Domain"] ?? "localhost";
        string port = builder.Configuration["Port"] ?? "7145";
        builder.WebHost.UseUrls($"{protocol}://{domain}:{port}");

        var app = builder.Build();

        app.UseEndpointDefinitions(app.Environment);

        // Define your Minimal API endpoints
        app.MapGet("/", () => "Hello from Minimal API hosted in Windows Service!");

        app.MapGet("/health", () => "Service is healthy");

        return app;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ApiService"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose managed state (managed objects).
            _webAppCts?.Dispose();
            _webHost?.Dispose();

            // Dispose unmanaged state (unmanaged objects) and override a finalizer below.
            // ...
        }
    }

    /// <summary>
    /// Finalizer for the <see cref="ApiService"/> class. Disposes of the object if not already disposed.
    /// </summary>
    ~ApiService()
    {
        Dispose(false);
    }
}