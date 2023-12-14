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
}
