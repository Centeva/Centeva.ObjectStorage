using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure";
    private const string AccountName = "AccountName";
    private const string AccountKey = "AccountKey";
    private const string Endpoint = "Endpoint";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        const string suffix = "core.windows.net";
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = connectionString.GetRequired(AccountKey).Replace(' ', '+');
        var endpoint = new Uri(connectionString.Get(Endpoint) ?? $"https://{accountName}.blob.{suffix}");

        return new AzureObjectStorage(accountName, accountKey, endpoint);
    }
}
