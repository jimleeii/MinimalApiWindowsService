using EndpointDefinition;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace MinimalApiWindowsService.Extensions.Helpers;

/// <summary>
/// A helper class with extension methods for adding plug-ins to the services collection.
/// </summary>
public static partial class PluginExtensionHelper
{
    /// <summary>
    /// A regex pattern that matches endpoint names in the form of "SomeController.SomeAction".
    /// </summary>
    [GeneratedRegex(ApiConstants.SwaggerPattern, RegexOptions.IgnoreCase, ApiConstants.US_EN)]
    private static partial Regex SwaggerRegex();

    /// <summary>
    /// Loads assemblies from DLL files located in the specified file path and
    /// attempts to add endpoint definitions from each assembly to the provided
    /// IServiceCollection. This method processes each DLL file in parallel.
    /// </summary>
    /// <param name="logger">The logger used to log information, warnings, and errors for the plug-in loading process.</param>
    /// <param name="filePaths">The directory path containing the DLL files to load.</param>
    /// <remarks>
    /// Each DLL is loaded and inspected for a type named "Program". If such a type is found,
    /// its endpoint definitions are added to the service collection. Exceptions during
    /// assembly loading are caught and logged.
    /// </remarks>
    public static Type?[] LoadFromFiles(ILogger logger, params string[] filePaths)
    {
        return filePaths
            .SelectMany(Directory.EnumerateFiles)
            .Select(filePath =>
            {
                try
                {
                    AssemblyLoadContext context = new(filePath, true);
                    return context.LoadFromAssemblyPath(filePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load assembly from {File}: {Message}", filePath, ex.Message);
                    return null;
                }
            })
            .Where(assembly => assembly != null)
            .Select(plugin => plugin!.GetType(nameof(Program), false))
            .Where(programType => programType != null)
            .AsParallel()
            .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount - 1))
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .Select(programType =>
            {
                try
                {
                    return programType;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error loading assembly from {File}: {Message}", programType!.Assembly.Location, ex.Message);
                    return null;
                }
            })
            .Where(programType => programType != null)
            .ToArray();
    }

    /// <summary>
    /// Add endpoint definitions.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="scanMarkers">The scan markers.</param>
    public static void AddEndpointDefinitions(IServiceCollection services, params Type[] scanMarkers)
    {
        var exportedTypes = scanMarkers
            .SelectMany(marker => marker.Assembly.ExportedTypes)
            .ToList();

        var endpointDefinitions = exportedTypes
            .Where(x => typeof(IEndpointDefinition).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !SwaggerRegex().IsMatch(x.Name))
            .Select(x => ActivatorUtilities.CreateInstance(services.BuildServiceProvider(), x))
            .Cast<IEndpointDefinition>()
            .ToList();

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.DefineServices(services);
        }

        services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);
    }
}