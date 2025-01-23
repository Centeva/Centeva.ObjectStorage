namespace Centeva.ObjectStorage.Azure.Blob;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the Azure Blob storage provider.
    /// </summary>
    public static StorageFactory UseAzureBlobStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AzureBlobConnectionFactory());
        return connectionFactory;
    }
}
