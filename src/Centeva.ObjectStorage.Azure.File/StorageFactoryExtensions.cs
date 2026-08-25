using Azure.Core;

using Centeva.ObjectStorage.DependencyInjection;

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

    /// <summary>
    /// Configure Azure File storage directly, without using a connection
    /// string, authenticating with a shared account key.
    /// </summary>
    public static ObjectStorageBuilder UseAzureFileStorage(this ObjectStorageBuilder builder, string accountName, string accountKey, string shareName, Uri? serviceUri = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new AzureFileConnectionFactory());

        return builder.UseStorage(new AzureFileStorage(accountName, accountKey, shareName, serviceUri));
    }

    /// <summary>
    /// Configure Azure File storage directly, without using a connection
    /// string, authenticating with the given identity.
    /// </summary>
    public static ObjectStorageBuilder UseAzureFileStorage(this ObjectStorageBuilder builder, string accountName, string shareName, TokenCredential identity, Uri? serviceUri = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new AzureFileConnectionFactory());

        return builder.UseStorage(new AzureFileStorage(accountName, shareName, identity, serviceUri));
    }
}
