using DropBear.Codex.Serialization.Helpers;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Serialization;

/// <summary>
///     Extension methods for setting up serialization services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the data serialization services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    /// <remarks>
    ///     This method adds services for serializing and deserializing data using JSON, MessagePack, and MemoryPack.
    ///     It also registers a service for checking if types are serializable with MessagePack.
    /// </remarks>
    public static IServiceCollection AddDataSerializationServices(this IServiceCollection services)
    {
        // Registers the different serialization services.
        services.AddSingleton<IJsonSerializer, CustomJsonSerializer>();
        services.AddSingleton<IMessagePackSerializer, CustomMessagePackSerializer>();
        services.AddSingleton<IMemoryPackSerializer, CustomMemoryPackSerializer>();

        // Register the compression helper
        services.AddSingleton<ICompressionHelper, CompressionHelper>();

        // Registers the DataSerializer service that supports JSON, MessagePack, and MemoryPack serialization.
        services.AddSingleton<IDataSerializer, DataSerializer>();

        // Register the factory method
        services.AddSingleton<Func<string, ISerializableChecker>>(serviceProvider => key =>
        {
            return key switch
            {
                "MessagePack" => serviceProvider.GetRequiredService<MessagePackCompatibilityChecker>(),
                "MemoryPack" => serviceProvider.GetRequiredService<MemoryPackSerializableChecker>(),
                _ => throw new KeyNotFoundException()
            };
        });

        // Register the checker implementations
        services.AddSingleton<MessagePackCompatibilityChecker>();
        services.AddSingleton<MemoryPackSerializableChecker>();

        return services;
    }
}
