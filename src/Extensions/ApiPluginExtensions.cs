using EndpointDefinition;
using NuGet.Frameworks;
using NuGet.Packaging;
using System.Reflection;

namespace MinimalApiWindowsService.Extensions;

/// <summary>
/// Contains extension methods for adding plugins to the services collection.
/// </summary>
public static class ApiPluginExtensions
{
    /// <summary>
    /// Adds plugins to the services collection by loading them from the given directory.
    /// </summary>
    /// <param name="services">The services collection to add to.</param>
    /// <param name="pluginPath">The path to the plugins to be loaded.</param>
    /// <exception cref="InvalidOperationException">Thrown if the given <paramref name="pluginPath"/> is not a valid file path.</exception>
    public static void AddApiPlugins(this IServiceCollection services, string pluginPath)
    {
        if (!Directory.Exists(pluginPath))
        {
            throw new InvalidOperationException($"PluginPath configuration is not a valid file path: {pluginPath}");
        }

        try
        {
            // Load the plugin assembly
            services.LoadFromFiles(pluginPath);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Error loading plugin: {ex.Message}");
            // Optionally, re-throw the exception or take other corrective actions
            throw; // Re-throwing to ensure the application doesn't continue with a broken plugin setup
        }
    }

    /// <summary>
    /// Adds the NuGet packages in the given directory to the services collection by extracting the DLLs from the packages and loading them as plugins.
    /// </summary>
    /// <param name="services">The services collection to add to.</param>
    /// <param name="pluginPath">The path to the NuGet packages to be loaded.</param>
    public static void AddApiNugetPlugins(this IServiceCollection services, string pluginPath)
    {
        if (!Directory.Exists(pluginPath))
        {
            throw new InvalidOperationException($"PluginPath configuration is not a valid file path: {pluginPath}");
        }

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

                // Load the plugin assembly
                services.LoadFromFiles(extractPath);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error loading plugin: {ex.Message}");
                // Optionally, re-throw the exception or take other corrective actions
                throw; // Re-throwing to ensure the application doesn't continue with a broken plugin setup
            }
        });
    }

    /// <summary>
    /// Loads assemblies from DLL files located in the specified file path and
    /// attempts to add endpoint definitions from each assembly to the provided
    /// IServiceCollection. This method processes each DLL file in parallel.
    /// </summary>
    /// <param name="services">The IServiceCollection to which endpoint definitions are added.</param>
    /// <param name="filePath">The directory path containing the DLL files to load.</param>
    /// <remarks>
    /// Each DLL is loaded and inspected for a type named "Program". If such a type is found,
    /// its endpoint definitions are added to the service collection. Exceptions during
    /// assembly loading are caught and logged.
    /// </remarks>
    private static void LoadFromFiles(this IServiceCollection services, string filePath)
    {
        Parallel.ForEach(Directory.EnumerateFiles(filePath, "*.dll"), file =>
        {
            try
            {
                var plugin = Assembly.LoadFrom(file);
                var programType = plugin.GetTypes().FirstOrDefault(t => string.Equals(t.Name, "Program", StringComparison.OrdinalIgnoreCase));
                if (programType != null)
                {
                    var endpointDefinitionTypes = programType.Assembly.ExportedTypes
                        .Where(type => typeof(IEndpointDefinition).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        .ToList();
                    services.AddEndpointDefinitions(programType);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions that may occur during assembly loading
                Console.WriteLine($"Error loading assembly from {file}: {ex.Message}");
            }
        });
    }
}