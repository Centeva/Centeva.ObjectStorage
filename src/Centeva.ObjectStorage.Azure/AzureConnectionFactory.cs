using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "azure";
    private const string Protocol = "DefaultEndpointsProtocol";
    private const string EndpointSuffix = "EndpointSuffix";
    private const string AccountName = "AccountName";
    private const string AccountKey = "AccountKey";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        // The connection string contains the parts that make up the URI endpoint
        // {DefaultEndpointsProtocol}://{AccountName}.blob.{EndpointSuffix}
        var protocol = connectionString.Get(Protocol) ?? "https";
        var suffix = connectionString.Get(EndpointSuffix) ?? "core.windows.net";
        var accountName = connectionString.GetRequired(AccountName);
        var accountKey = connectionString.GetRequired(AccountKey).Replace(' ', '+');
        var endpoint = new Uri($"{protocol}://{accountName}.blob.{suffix}");

        return new AzureObjectStorage(accountName, accountKey, endpoint);
    }
}
