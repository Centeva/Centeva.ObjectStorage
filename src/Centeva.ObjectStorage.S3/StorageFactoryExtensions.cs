using Centeva.ObjectStorage.ConnectionString;

namespace Centeva.ObjectStorage.S3;
public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the S3 storage provider.
    /// </summary>
    public static StorageFactory UseS3CompatibleStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new S3ConnectionFactory());

        return connectionFactory;
    }
}
