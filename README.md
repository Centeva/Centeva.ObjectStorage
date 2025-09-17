# Centeva Object Storage Library

Centeva.ObjectStorage is a .NET 6+ / .NET Standard library that provides a
generic interface to local or cloud-hosted object ("blob") storage providers.

Supported providers are:

* Amazon AWS S3 (and anything compatible with its API such as
  [MinIO](https://min.io/)
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
* `Centeva.ObjectStorage.Azure.FileShare`

Create an instance of `StorageFactory` and register the providers you require,
then build an instance of `IObjectStorage` using a connection string:

```csharp
var factory = new StorageFactory()
    .UseAwsS3Storage()
    .UseAzureBlobStorage()
    .UseAzureFileShareStorage()
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
var storageFromConnectionString = factory.GetConnection("azure.files://share=myfiles;accountName=myaccount;accountKey=myAccountKey");
var storageFromConstructor = new AzureFileShareStorage("accountName", "accountKey", "shareName");

// Google Cloud Storage
var storageFromConnectionString = factory.GetConnection("google.storage://bucket=myfiles;credentialsFilePath=/path/to/creds.json");
var storageFromConnectionString2 = factory.GetConnection("google.storage://bucket=myfiles;credentials=base64EncodedCredentialsJson");
var storageFromConstructor = GoogleObjectStorage.CreateFromCredentialsFile("bucketName", "/path/to/creds.json");
var storageFromConstructor2 = GoogleObjectStorage.CreateFromCredentialsJson("bucketName", "credentialsJsonString");

// MinIO (using AWS S3 provider)
var storageFromConnectionString = factory.GetConnection("aws.s3://endpoint=http://localhost:9000;region=us-east-1;bucket=myfiles;accessKey=myAccount;secretKey=myPassword");
var storageFromConstructor = new AwsS3ObjectStorage("myfiles", "us-east-1", "http://localhost:9000", "myAccount", "myPassword");
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
