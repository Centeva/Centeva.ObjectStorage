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
    private const string ClientId = "clientId";
    private const string Endpoint = "endpoint";
    private const string Container = "container";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName
            && connectionString.ProviderName != LegacyProviderName)
            return null;

        var container = connectionString.GetRequired(Container);
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = (connectionString.Get(AccountKey) ?? "").Replace(' ', '+');
        var clientId = connectionString.Get(ClientId);
        var endpoint = connectionString.Get(Endpoint);

        // If we have an account key, we use shared key authentication
        if (accountKey is not null and not "") {
            return new AzureBlobObjectStorage(accountName, accountKey, container, endpoint is null ? null : new Uri(endpoint));
        }

        // If we don't specify which identity to use, we default to DefaultAzureCredential which will try to use the running environment's identity
        TokenCredential identity = new DefaultAzureCredential();

        // If we have a client ID, we use managed identity authentication
        if (clientId is not null and not "") {
            identity = new ManagedIdentityCredential(clientId);
        }

        return new AzureBlobObjectStorage(accountName, container, identity, endpoint is null ? null : new Uri(endpoint));
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