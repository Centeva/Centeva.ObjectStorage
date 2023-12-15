# Centeva Object Storage Library

Centeva.ObjectStorage is a .NET 6+ library that provides a generic interface to local or
cloud-hosted object ("blob") storage providers.

Supported providers are:

* Amazon AWS S3 (and anything compatible with its API such as [MinIO](https://min.io/)
* Google Cloud Storage
* Azure Blob Storage

## Built With

* [.NET 6](https://dot.net)

## Getting Started

Import the `Centeva.ObjectStorage` package to your projects where needed, along
with the desired provider sub-packages:

* `Centeva.ObjectStorage.GCP`
* `Centeva.ObjectStorage.AWS`
* `Centeva.ObjectStorage.Azure.Blob`

Create an instance of `StorageFactory` and register providers, then build an
instance of `IObjectStorage` using a connection string:

```csharp
var factory = new StorageFactory()
    .UseGoogleCloudStorage()
    .UseAwsS3Storage();

var storage = factory.GetConnection('provider://key1=value1;key2=value2');
```

**TODO:** Write API documentation

## Contributing

Please use a Pull Request to suggest changes to this library.  As this is a
shared library, strict semantic versioning rules should be followed to avoid
unexpected breaking changes.

### Running Tests

From Windows, use the `dotnet test` command, or your Visual Studio Test
Explorer.

### Deployment

This library is versioned by [GitVersion](https://gitversion.net/).  Create a
Git tag for an official release (e.g., "v1.0.0").  Version numbers can be
incremented via commit message using the [GitVersion
approaches](https://gitversion.net/docs/reference/version-increments).
