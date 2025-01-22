using Azure.Core;
using Azure.Identity;

using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure.Blob;

public class AzureBlobConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure.blob";
    private const string LegacyProviderName = "azure";
    private const string AccountName = "accountName";
    private const string AccountKey = "accountKey";
    private const string Endpoint = "endpoint";
    private const string Container = "container";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName
            && connectionString.ProviderName != LegacyProviderName)
            return null;

        var container = connectionString.GetRequired(Container);
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = connectionString.GetRequired(AccountKey).Replace(' ', '+');
        var endpoint = connectionString.Get(Endpoint);

        return new AzureBlobObjectStorage(accountName, accountKey, container, endpoint is null ? null : new Uri(endpoint));
    }
}

public class AzureBlobIdentityConnectionFactory: IConnectionFactory {
    private const string ProviderName = "azure.blob.identity";
    private const string AccountName = "accountName";
    private const string Endpoint = "endpoint";
    private const string Container = "container";
    private const string ClientId = "clientId";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString) {
        if (connectionString.ProviderName != ProviderName)
            return null;

        var container = connectionString.GetRequired(Container);
        var accountName = connectionString.GetRequired(AccountName);
        var endpoint = connectionString.Get(Endpoint);
        var clientId = connectionString.Get(ClientId);

        TokenCredential? identity = null;
        if (clientId is not null) {
            identity = new ManagedIdentityCredential(clientId);
        }

        return new AzureBlobObjectStorage(accountName, container, identity, endpoint is null ? null : new Uri(endpoint));
    }
}