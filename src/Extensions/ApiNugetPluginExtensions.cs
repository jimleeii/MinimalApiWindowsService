using EndpointDefinition;
using MinimalApiWindowsService.Extensions.Helpers;
using NuGet.Frameworks;
using NuGet.Packaging;
using System.Collections.Concurrent;

namespace MinimalApiWindowsService.Extensions;

/// <summary>
/// Contains extension methods for adding plug-ins to the services collection.
/// </summary>
public static class ApiNugetPluginExtensions
{
    // The logger factory used to create loggers
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(static builder => builder.AddConsole());

    // The logger used to log messages
    private static readonly ILogger _logger = _loggerFactory.CreateLogger(typeof(ApiPluginExtensions).FullName!);

    /// <summary>
    /// Adds the NuGet packages in the given directory to the services collection by extracting the DLLs from the packages and loading them as plug-ins.
    /// </summary>
    /// <param name="services">The services collection to add to.</param>
    /// <param name="pluginPath">The path to the NuGet packages to be loaded.</param>
    public static void AddApiNugetPluginsEndpointDefinitions(this IServiceCollection services, string pluginPath)
    {
        if (!Directory.Exists(pluginPath))
        {
            throw new InvalidOperationException($"PluginPath configuration is not a valid file path: {pluginPath}");
        }

        var nugetPackagePathes = new ConcurrentBag<string>();

        var files = Directory.GetFiles(pluginPath, "*.nupkg");
        Parallel.ForEach(files, file =>
        {
            try
            {
                // Extract the DLLs from the NuGet package
                var extractPath = Path.Combine(pluginPath, Path.GetFileNameWithoutExtension(file));

                // Extract NuGet package
                using var packageReader = new PackageArchiveReader(file);
                var nearestFramework = packageReader.GetLibItems()
                    .Where(f => f.TargetFramework.Version >= NuGetFramework.Parse("net8.0").Version)
                    .OrderBy(f => f.TargetFramework.Version)
                    .FirstOrDefault();

                if (nearestFramework != null)
                {
                    var assemblyFiles = nearestFramework.Items.Where(i => i.EndsWith(".dll"));
                    Directory.CreateDirectory(extractPath);

                    foreach (var item in assemblyFiles)
                    {
                        var assemblyName = Path.GetFileName(item);
                        var outputPath = Path.Combine(extractPath, assemblyName);
                        using var stream = packageReader.GetStream(item);
                        using var fileStream = new FileStream(outputPath, FileMode.Create);
                        stream.CopyTo(fileStream);
                    }
                }

                nugetPackagePathes.Add(extractPath);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                _logger.LogError(ex, "Error loading plugin: {Message}", ex.Message);
                // Optionally, re-throw the exception or take other corrective actions
                throw; // Re-throwing to ensure the application doesn't continue with a broken plugin setup
            }
        });

        // Load the plug-in assembly
        var programTypes = PluginExtensionHelper.LoadFromFiles(_logger, [.. nugetPackagePathes]);
        PluginExtensionHelper.AddEndpointDefinitions(services, programTypes!);
    }

    /// <summary>
    /// Integrates user-defined API plug-ins into the web application by retrieving endpoint
    /// definitions from the service collection and defining them in the specified web application.
    /// </summary>
    /// <param name="app">The web application to which the endpoints will be added.</param>
    /// <param name="env">The hosting environment in which the application is running.</param>
    public static void UseApiNugetPluginsEndpointDefinitions(this WebApplication app, IWebHostEnvironment env)
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