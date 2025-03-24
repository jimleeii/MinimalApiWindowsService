using EndpointDefinition;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace MinimalApiWindowsService.EndpointDefinitions;

/// <summary>
/// The swagger endpoint definition.
/// </summary>
public class SwaggerEndpointDefinition : IEndpointDefinition
{
    // The title of the API
    private readonly string Title = Assembly.GetEntryAssembly()!.GetName().Name!;
    // The version of the API
    private const string Version = "v1";

    /// <summary>
    /// Defines the endpoints.
    /// </summary>
    /// <param name="app">The app.</param>
    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Title} {Version}"));
        }
    }

    /// <summary>
    /// Defines the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void DefineServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(Version, new OpenApiInfo { Title = Title, Version = Version });
        });
    }
}