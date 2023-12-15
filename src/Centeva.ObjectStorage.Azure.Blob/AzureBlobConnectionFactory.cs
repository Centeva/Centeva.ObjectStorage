using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure.Blob;

public class AzureBlobConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure.blob";
    private const string LegacyProviderName = "azure";
    private const string AccountName = "AccountName";
    private const string AccountKey = "AccountKey";
    private const string Endpoint = "Endpoint";
    private const string Container = "Container";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName
            && connectionString.ProviderName != LegacyProviderName)
            return null;

        const string suffix = "core.windows.net";
        var container = connectionString.GetRequired(Container);
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = connectionString.GetRequired(AccountKey).Replace(' ', '+');
        var endpoint = new Uri(connectionString.Get(Endpoint) ?? $"https://{accountName}.blob.{suffix}");

        return new AzureBlobObjectStorage(accountName, accountKey, container, endpoint);
    }
}
