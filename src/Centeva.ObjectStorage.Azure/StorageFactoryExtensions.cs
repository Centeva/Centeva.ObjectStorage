namespace Centeva.ObjectStorage.Azure;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the Azure storage provider.
    /// </summary>
    public static StorageFactory UseAzureCompatibleStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AzureConnectionFactory());
        return connectionFactory;
    }
}
