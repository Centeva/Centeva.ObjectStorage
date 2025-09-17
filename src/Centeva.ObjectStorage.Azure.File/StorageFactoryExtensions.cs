namespace Centeva.ObjectStorage.Azure.File;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the Azure File storage provider.
    /// </summary>
    public static StorageFactory UseAzureFileStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AzureFileConnectionFactory());
        return connectionFactory;
    }
}
