using DropBear.Codex.Serialization.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Serialization.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerializationServices(this IServiceCollection services,
        Action<SerializationBuilder> configure)
    {
        var builder = new SerializationBuilder();
        configure(builder);
        var serializer = builder.Build();

        services.AddSingleton(serializer);
        return services;
    }
}
