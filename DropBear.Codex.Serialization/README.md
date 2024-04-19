# DropBear.Codex.Serialization

## Overview
DropBear.Codex.Serialization provides a robust and highly configurable serialization framework, supporting multiple serialization formats such as JSON and MessagePack. It integrates advanced features such as compression, encryption, and encoding to meet modern application needs for data handling and security.

## Features
- **Flexible Serialization & Deserialization**: Supports multiple formats including JSON and MessagePack.
- **Configurable Pipeline**: Customize serialization processes with encryption, compression, and encoding through a fluent API.
- **Encryption Support**: Integrates RSA and AES encryption to ensure data security during serialization.
- **Compression Options**: Includes support for GZip and Deflate compression algorithms to optimize data size.
- **Encoding Techniques**: Supports Base64 and Hexadecimal encoding to enhance data interoperability.
- **Stream Serialization**: Facilitates serialization and deserialization directly from and to streams, improving performance for large data sets.

## Getting Started

### Setup
To utilize the serialization capabilities, you need to configure and build the serializer using the `SerializationBuilder`. Here's how you can set it up in your application:

```csharp
var builder = new SerializationBuilder()
    .WithSerializer<JsonSerializer>() // Choose the serializer
    .WithCompression<GZipCompressionProvider>() // Specify compression
    .WithEncryption("path/to/publicKey", "path/to/privateKey") // Configure encryption
    .WithEncoding<Base64EncodingProvider>(); // Set encoding

var serializer = builder.Build();
```
Registration in Dependency Injection (DI)
You can easily integrate the serialization builder into a DI container to use throughout your application:

```csharp
services.AddSingleton<ISerializer>(_ => new SerializationBuilder()
.WithSerializer<JsonSerializer>()
.WithCompression<GZipCompressionProvider>()
.WithEncryption("path/to/publicKey", "path/to/privateKey")
.WithEncoding<Base64EncodingProvider>()
.Build());
```
Usage
Use the serializer instance created by the SerializationBuilder to serialize and deserialize data:

```csharp
var myObject = new MyDataClass();
var serializedData = await serializer.SerializeAsync(myObject);
var deserializedObject = await serializer.DeserializeAsync<MyDataClass>(serializedData);
This setup ensures that the data is serialized, compressed, encrypted, and encoded seamlessly.
```

## Contributing
Contributions are welcome! Please fork the repository, make your changes, and submit a pull request. For major changes, please open an issue first to discuss what you would like to change.

## License
This project is licensed under [GNU Lesser General Public License](https://www.gnu.org/licenses/lgpl-3.0.en.html).

## Development Status

**Note:** This library is relatively new and under active development. While it is being developed with robustness and best practices in mind, it may still be evolving.

We encourage you to test thoroughly and contribute if possible before using this library in a production environment. The API and features may change as feedback is received and improvements are made. We appreciate your understanding and contributions to making this library better!

Please use the following link to report any issues or to contribute: [GitHub Issues](https://github.com/tkuchel/DropBear.Codex.Serialization/issues).
