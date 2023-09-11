using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        return connectionString.ProviderName != ProviderName
            ? null
            : (IObjectStorage)new AzureObjectStorage(connectionString);
    }
}
