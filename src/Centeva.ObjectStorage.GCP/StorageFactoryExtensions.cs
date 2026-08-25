using Centeva.ObjectStorage.DependencyInjection;

namespace Centeva.ObjectStorage.GCP;
public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the GCP storage provider.
    /// </summary>
    public static TRegistry UseGoogleCloudStorage<TRegistry>(this TRegistry registry) where TRegistry : IObjectStorageProviderRegistry
    {
        registry.Register(new GoogleConnectionFactory());

        return registry;
    }

    /// <summary>
    /// Configure Google Cloud Storage directly, without using a connection
    /// string, using credentials read from the given file path.
    /// </summary>
    public static ObjectStorageBuilder UseGoogleCloudStorageFromCredentialsFile(this ObjectStorageBuilder builder, string bucketName, string credentialsFilePath)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new GoogleConnectionFactory());

        return builder.UseStorage(GoogleObjectStorage.CreateFromCredentialsFile(bucketName, credentialsFilePath));
    }

    /// <summary>
    /// Configure Google Cloud Storage directly, without using a connection
    /// string, using the given credentials JSON.
    /// </summary>
    public static ObjectStorageBuilder UseGoogleCloudStorageFromCredentialsJson(this ObjectStorageBuilder builder, string bucketName, string credentialsJsonString)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new GoogleConnectionFactory());

        return builder.UseStorage(GoogleObjectStorage.CreateFromCredentialsJson(bucketName, credentialsJsonString));
    }
}
