using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Serializer that applies encryption to serialized data before serialization and decryption after deserialization.
/// </summary>
public class EncryptedSerializer : ISerializer
{
    private readonly IEncryptor _encryptor;
    private readonly ISerializer _innerSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptedSerializer" /> class.
    /// </summary>
    /// <param name="innerSerializer">The inner serializer.</param>
    /// <param name="encryptor">The encryptor to use for encryption and decryption.</param>
    public EncryptedSerializer(ISerializer innerSerializer, IEncryptor encryptor)
    {
        _innerSerializer = innerSerializer;
        _encryptor = encryptor;
    }

    /// <inheritdoc />
    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        // Serialize the value using the inner serializer
        var serializedData = await _innerSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);

        // Encrypt the serialized data using the provided encryptor
        return await _encryptor.EncryptAsync(serializedData, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        // Decrypt the data using the provided encryptor
        var decryptedData = await _encryptor.DecryptAsync(data, cancellationToken).ConfigureAwait(false);

        // Deserialize the decrypted data using the inner serializer
        return await _innerSerializer.DeserializeAsync<T>(decryptedData, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Deserializes raw byte data after decrypting it, returning the decrypted byte array.
    /// </summary>
    /// <param name="data">The encrypted byte data to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the deserialization operation.</param>
    /// <returns>The decrypted byte array from the provided encrypted data.</returns>
    public async Task<byte[]> DeserializeRawBytesAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        // Decrypt the data using the provided encryptor
        var decryptedData = await _encryptor.DecryptAsync(data, cancellationToken).ConfigureAwait(false);

        // Return the decrypted byte array directly
        return decryptedData;
    }
}
