using EndpointDefinition;

namespace MinimalApiWindowsService.Extensions;

/// <summary>
/// Contains extension methods for adding plug-ins to the services collection.
/// </summary>
public static class ApiPluginExtensions
{
    // The logger factory used to create loggers
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(static builder => builder.AddConsole());

    // The logger used to log messages
    private static readonly ILogger _logger = _loggerFactory.CreateLogger(typeof(ApiPluginExtensions).FullName!);

    /// <summary>
    /// Adds plug-ins to the services collection by loading them from the given directory.
    /// </summary>
    /// <param name="services">The services collection to add to.</param>
    /// <param name="pluginPath">The path to the plug-ins to be loaded.</param>
    /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="pluginPath"/> is not a valid file path.</exception>
    public static void AddApiPluginsEndpointDefinitions(this IServiceCollection services, string pluginPath)
    {
        if (!Directory.Exists(pluginPath))
        {
            throw new InvalidOperationException($"PluginPath configuration is not a valid file path: {pluginPath}");
        }

        try
        {
            // Load the plug-in assembly
            var programTypes = PluginExtensionHelper.LoadFromFiles(_logger, pluginPath);
            PluginExtensionHelper.AddEndpointDefinitions(services, programTypes!);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            _logger.LogError(ex, "Error loading plugin: {Message}", ex.Message);
            // Optionally, re-throw the exception or take other corrective actions
            throw; // Re-throwing to ensure the application doesn't continue with a broken plug-in setup
        }
    }

    /// <summary>
    /// Integrates user-defined API plug-ins into the web application by retrieving endpoint
    /// definitions from the service collection and defining them in the specified web application.
    /// </summary>
    /// <param name="app">The web application to which the endpoints will be added.</param>
    /// <param name="env">The hosting environment in which the application is running.</param>
    public static void UseApiPluginsEndpointDefinitions(this WebApplication app, IWebHostEnvironment env)
    {
        // Get all endpoint definitions from the API DLLs
        var endpointDefinitions = app.Services.GetServices<IReadOnlyCollection<IEndpointDefinition>>().First();

        // Add each endpoint definition to the Swagger document
        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.DefineEndpoints(app, env);
        }
    }
}