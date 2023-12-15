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
}
