using DropBear.Codex.Serialization.Factories;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Extensions;

public static class SerializerFactoryExtensions
{
    public static void RegisterSerializer<T>(this SerializerFactory factory) where T : ISerializer =>
        SerializerFactory.RegisteredSerializers.Add(typeof(T), typeof(T));

    public static void RegisterSerializer<TSerializer, TImplementation>()
        where TSerializer : ISerializer
        where TImplementation : class, TSerializer =>
        SerializerFactory.RegisteredSerializers.Add(typeof(TSerializer), typeof(TImplementation));
}
