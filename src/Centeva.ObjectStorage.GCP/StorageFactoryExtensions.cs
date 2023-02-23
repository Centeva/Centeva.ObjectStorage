using Centeva.ObjectStorage.ConnectionString;

namespace Centeva.ObjectStorage.GCP;
public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the GCP storage provider.
    /// </summary>
    public static StorageFactory UseGoogleCloudStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new GoogleConnectionFactory());

        return connectionFactory;
    }
}
