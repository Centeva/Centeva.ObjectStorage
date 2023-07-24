using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure";
    private const string Endpoint = "endpoint";
    private const string Container = "container";
    private const string AccountName = "accountName";
    private const string AccountKey = "accountKey";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        var container = connectionString.GetRequired(Container);
        var endpoint = connectionString.Get(Endpoint);
        var accountName = connectionString.Get(AccountName);
        var accountKey = connectionString.Get(AccountKey);

        return new AzureObjectStorage(container, endpoint, accountName, accountKey);
    }
}
