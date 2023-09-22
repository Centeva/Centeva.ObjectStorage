namespace Centeva.ObjectStorage.Azure;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the Azure storage provider.
    /// </summary>
    public static StorageFactory UseAzureBlobCompatibleStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AzureBlobConnectionFactory());
        return connectionFactory;
    }
}
