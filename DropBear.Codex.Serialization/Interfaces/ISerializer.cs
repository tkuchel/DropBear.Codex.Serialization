﻿namespace DropBear.Codex.Serialization.Interfaces;

public interface ISerializer
{
    Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default);
    Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default);
}
