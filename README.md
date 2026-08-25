# Centeva Object Storage Library

Centeva.ObjectStorage is a .NET 6+ / .NET Standard library that provides a
generic interface to local or cloud-hosted object ("blob") storage providers.

Supported providers are:

* Amazon AWS S3 (and anything compatible with its API such as
  [MinIO](https://min.io/))
* Google Cloud Storage
* Azure Blob Storage
* Azure File Share Storage
* Local disk

## Built With

* [.NET 8](https://dot.net)

## Getting Started

Import the `Centeva.ObjectStorage` package to your projects where needed, along
with the desired provider sub-packages:

* `Centeva.ObjectStorage.GCP`
* `Centeva.ObjectStorage.AWS`
* `Centeva.ObjectStorage.Azure.Blob`
* `Centeva.ObjectStorage.Azure.File`

Create an instance of `StorageFactory` and register the providers you require,
then build an instance of `IObjectStorage` using a connection string:

```csharp
var factory = new StorageFactory()
    .UseAwsS3Storage()
    .UseAzureBlobStorage()
    .UseAzureFileStorage()
    .UseGoogleCloudStorage();

var storage = factory.GetConnection("provider://key1=value1;key2=value2");
```

In modern .NET applications, you will likely do this as part of service
registration in Program.cs, obtaining the connection string from configuration.

In some cases you may prefer to instantiate storage providers directly rather
than using connection strings.  Each provider's constructor allows the needed
parameters to be provided.

### Examples

```csharp
// Local Disk
var storageFromConnectionString = factory.GetConnection("disk://path=C:\\temp\\files");
var storageFromConstructor = new DiskObjectStorage("C:\\temp\\files");

// AWS S3
var storageFromConnectionString = factory.GetConnection("aws.s3://bucket=myfiles;accessKey=mykey;secretKey=secret");
var storageFromConstructor = new AwsS3ObjectStorage("myfiles", "regionName", "endpointUrl", "accessKey", "secret");

// Azure Blob Storage
var storageFromConnectionString = factory.GetConnection("azure.blob://container=myfiles;accountName=myaccount;accountKey=myAccountKey");
var storageFromConstructor = new AzureBlobObjectStorage("accountName", "accountKey", "containerName");

// Azure FileShare Storage
var storageFromConnectionString = factory.GetConnection("azure.file://share=myfiles;accountName=myaccount;accountKey=myAccountKey");
var storageFromConstructor = new AzureFileStorage("accountName", "accountKey", "shareName");

// Google Cloud Storage
var storageFromConnectionString = factory.GetConnection("google.storage://bucket=myfiles;credentialsFilePath=/path/to/creds.json");
var storageFromConnectionString2 = factory.GetConnection("google.storage://bucket=myfiles;credentials=base64EncodedCredentialsJson");
var storageFromConstructor = GoogleObjectStorage.CreateFromCredentialsFile("bucketName", "/path/to/creds.json");
var storageFromConstructor2 = GoogleObjectStorage.CreateFromCredentialsJson("bucketName", "credentialsJsonString");

// MinIO (using AWS S3 provider)
var storageFromConnectionString = factory.GetConnection("aws.s3://endpoint=http://localhost:9000;region=us-east-1;bucket=myfiles;accessKey=myAccount;secretKey=myPassword");
var storageFromConstructor = new AwsS3ObjectStorage("myfiles", "us-east-1", "http://localhost:9000", "myAccount", "myPassword");
```

### Dependency Injection

The `AddObjectStorage` extension method registers an `IObjectStorage` singleton
with the built-in .NET dependency injection container:

```csharp
builder.Services.AddObjectStorage(config => config
    .UseAwsS3Storage()
    .UseAzureBlobStorage()
    .UseAzureFileStorage()
    .UseGoogleCloudStorage()
    .UseConnectionString(builder.Configuration.GetConnectionString("ObjectStorage")!));
```

Providers can also be configured directly, without a connection string:

```csharp
// AWS S3
builder.Services.AddObjectStorage(config =>
    config.UseAwsS3Storage("myfiles", "us-east-1", null, "accessKey", "secret"));

// Azure Blob Storage
builder.Services.AddObjectStorage(config =>
    config.UseAzureBlobStorage("accountName", "accountKey", "containerName"));

builder.Services.AddObjectStorage(config =>
    config.UseAzureBlobStorage("accountName", "containerName", new DefaultAzureCredential()));

// Azure FileShare Storage
builder.Services.AddObjectStorage(config =>
    config.UseAzureFileStorage("accountName", "accountKey", "shareName"));

builder.Services.AddObjectStorage(config =>
    config.UseAzureFileStorage("accountName", "shareName", new DefaultAzureCredential()));

// Google Cloud Storage
builder.Services.AddObjectStorage(config =>
    config.UseGoogleCloudStorageFromCredentialsFile("bucketName", "/path/to/creds.json"));

builder.Services.AddObjectStorage(config =>
    config.UseGoogleCloudStorageFromCredentialsJson("bucketName", credentialsJsonString));
```

You can also supply any `IObjectStorage` instance you have constructed
yourself:

```csharp
builder.Services.AddObjectStorage(config =>
    config.UseStorage(new DiskObjectStorage(@"C:\temp\files")));
```

Then inject `IObjectStorage` where you need it:

```csharp
public class MyService(IObjectStorage storage)
{
    public Task<bool> ExistsAsync(StoragePath path) => storage.ExistsAsync(path);
}
```

If the configured provider supports optional capabilities, the corresponding
interfaces are registered against the same instance, so you can inject them
directly:

```csharp
public class MyService(ISupportsSignedUrls signedUrls)
{
    public Task<Uri> GetUrlAsync(StoragePath path) => signedUrls.GetDownloadUrlAsync(path);
}
```

`ISupportsSignedUrls` and `ISupportsMetadata` are only registered when the
provider actually implements them.

If your application needs more than one storage configuration, register them as
keyed services:

```csharp
builder.Services.AddKeyedObjectStorage("documents", config => config
    .UseAwsS3Storage()
    .UseConnectionString(builder.Configuration.GetConnectionString("Documents")!));

builder.Services.AddKeyedObjectStorage("thumbnails", config => config
    .UseAzureBlobStorage()
    .UseConnectionString(builder.Configuration.GetConnectionString("Thumbnails")!));
```

Then resolve them by key:

```csharp
public class MyService([FromKeyedServices("documents")] IObjectStorage storage)
{
    public Task<bool> ExistsAsync(StoragePath path) => storage.ExistsAsync(path);
}
```

Capability interfaces are registered with the same key.

### MinIO and macOS Compatibility

When using MinIO or other S3-compatible storage services with custom endpoints on macOS,
the library automatically uses `AuthenticationRegion` instead of `RegionEndpoint` to avoid
DNS resolution conflicts with the AWS SDK. This ensures proper functionality across all
platforms (Windows, Linux, and macOS) without requiring any special configuration.

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
