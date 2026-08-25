using Centeva.ObjectStorage.DependencyInjection;

namespace Centeva.ObjectStorage.AWS;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the AWS S3 storage provider.
    /// </summary>
    public static TRegistry UseAwsS3Storage<TRegistry>(this TRegistry registry) where TRegistry : IObjectStorageProviderRegistry
    {
        registry.Register(new AwsS3ConnectionFactory());

        return registry;
    }

    /// <summary>
    /// Register the AWS S3 storage provider.
    /// </summary>
    [Obsolete("UseS3CompatibleStorage is deprecated, please use UseAwsS3Storage instead.")]
    public static TRegistry UseS3CompatibleStorage<TRegistry>(this TRegistry registry) where TRegistry : IObjectStorageProviderRegistry => UseAwsS3Storage(registry);

    /// <summary>
    /// Configure AWS S3 storage directly, without using a connection string.
    /// </summary>
    public static ObjectStorageBuilder UseAwsS3Storage(this ObjectStorageBuilder builder, string bucketName, string? region, string? endpoint, string accessKey, string secretKey)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new AwsS3ConnectionFactory());

        return builder.UseStorage(new AwsS3ObjectStorage(bucketName, region, endpoint, accessKey, secretKey));
    }
}
