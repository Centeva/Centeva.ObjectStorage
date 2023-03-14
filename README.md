# Centeva Object Storage Library

Centeva.ObjectStorage is a .NET 6+ library that provides a generic interface to local or
cloud-hosted object ("blob") storage providers.

Supported providers are:

* Amazon AWS S3 (and anything compatible with its API such as MinIO)
* Google Cloud Storage

## Built With

* [.NET 6](https://dot.net)

## Getting Started

Add the Centeva NuGet repository to your own solution by adding a
[nuget.config](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file)
file in the same folder as your solution (.sln):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="Centeva Public" value="https://builds.centeva.com/guestAuth/app/nuget/feed/CentevaPackages/default/v3/index.json" />
  </packageSources>
</configuration>
```

Import the `Centeva.ObjectStorage` package to your projects where needed, along
with the desired provider sub-packages:

* `Centeva.ObjectStorage.GCP`
* `Centeva.ObjectStorage.S3`

Create an instance of `StorageFactory` and register providers, then build an
instance of `IObjectStorage` using a connection string:

```csharp
var factory = new StorageFactory()
    .UseGoogleCloudStorage()
    .UseS3CompatibleStorage();

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

## Resources

Take a look at <https://bitbucket.org/centeva/centeva.templates> for more
implementation details.
