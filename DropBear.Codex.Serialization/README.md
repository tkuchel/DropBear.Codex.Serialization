# Serialization Project

## Overview
This project provides advanced serialization and deserialization capabilities, supporting formats like JSON, MessagePack, and MemoryPack. It includes performance optimizations, error handling, and supports compression and encoding options.

## Features
- **Serialization & Deserialization**: Supports JSON, MessagePack, and MemoryPack with optional compression.
- **Customizable**: Easily extendable for additional formats or compression algorithms.
- **Error Handling**: Robust error handling and logging for debugging and operational insights.

## Getting Started
To use the serializers, instantiate the `DataSerializer` class with dependencies on specific serializers (`JsonSerializer`, `MessagePackSerializer`, `MemoryPackSerializer`) and a compression helper.

```csharp
var logger = // Obtain ILogger instance
var compressionHelper = new CompressionHelper(logger);
var jsonSerializer = new JsonSerializer(logger, compressionHelper);
var messagePackSerializer = new MessagePackSerializer(logger, compressionHelper);
var memoryPackSerializer = new MemoryPackSerializer(logger, compressionHelper);
var dataSerializer = new DataSerializer(logger, jsonSerializer, messagePackSerializer, memoryPackSerializer);

Alternatively use the service collection extension method to register the serializers and compression helper.

Simply add the following line to your `Startup.cs` file:

services.AddDataSerializationServices();

```

## Usage

Use the dataSerializer instance to serialize and deserialize data to/from JSON, MessagePack, and MemoryPack formats, with or without compression.

## Contributing

Contributions are welcome! Please open an issue or pull request to suggest improvements or add new features.
License

This project is licensed under [GNU Lesser General Public License](https://www.gnu.org/licenses/lgpl-3.0.en.html).