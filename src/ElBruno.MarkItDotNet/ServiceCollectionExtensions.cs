using ElBruno.MarkItDotNet.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet;

/// <summary>
/// Extension methods for registering MarkItDotNet services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet services (ConverterRegistry, MarkdownService, built-in converters) to the service collection.
    /// </summary>
    public static IServiceCollection AddMarkItDotNet(
        this IServiceCollection services,
        Action<MarkItDotNetOptions>? configure = null)
    {
        var options = new MarkItDotNetOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        var registry = new ConverterRegistry();

        // Register built-in converters
        registry.Register(new PlainTextConverter());

        services.AddSingleton(registry);
        services.AddSingleton<MarkdownService>();

        return services;
    }
}
