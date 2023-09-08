using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure";
    private const string Container = "Container";
    private const string DefaultEndpointsProtocol = "DefaultEndpointsProtocol";
    private const string AccountName = "AccountName";
    private const string AccountKey = "AccountKey";
    private const string EndpointSuffix = "EndpointSuffix";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        var container = connectionString.GetRequired(Container);
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = connectionString.GetRequired(AccountKey);

        return new AzureObjectStorage(container, connectionString as AzureObjectStorageConnectionString, accountName, accountKey);
    }
}
