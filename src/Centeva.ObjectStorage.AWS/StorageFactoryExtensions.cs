using Centeva.ObjectStorage.DependencyInjection;

namespace Centeva.ObjectStorage.AWS;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the AWS S3 storage provider.
    /// </summary>
    public static StorageFactory UseAwsS3Storage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AwsS3ConnectionFactory());

        return connectionFactory;
    }

    /// <summary>
    /// Register the AWS S3 storage provider.
    /// </summary>
    [Obsolete("UseS3CompatibleStorage is deprecated, please use UseAwsS3Storage instead.")]
    public static StorageFactory UseS3CompatibleStorage(this StorageFactory connectionFactory) => UseAwsS3Storage(connectionFactory);

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
