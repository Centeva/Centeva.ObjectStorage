using global::Azure.Core;

using Centeva.ObjectStorage.DependencyInjection;

namespace Centeva.ObjectStorage.Azure.Blob;

public static class StorageFactoryExtensions
{
    /// <summary>
    /// Register the Azure Blob storage provider.
    /// </summary>
    public static StorageFactory UseAzureBlobStorage(this StorageFactory connectionFactory)
    {
        connectionFactory.Register(new AzureBlobConnectionFactory());
        return connectionFactory;
    }

    /// <summary>
    /// Configure Azure Blob storage directly, without using a connection
    /// string, authenticating with a shared account key.
    /// </summary>
    public static ObjectStorageBuilder UseAzureBlobStorage(this ObjectStorageBuilder builder, string accountName, string accountKey, string container, Uri? serviceUri = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new AzureBlobConnectionFactory());

        return builder.UseStorage(new AzureBlobObjectStorage(accountName, accountKey, container, serviceUri));
    }

    /// <summary>
    /// Configure Azure Blob storage directly, without using a connection
    /// string, authenticating with the given identity.
    /// </summary>
    public static ObjectStorageBuilder UseAzureBlobStorage(this ObjectStorageBuilder builder, string accountName, string container, TokenCredential identity, Uri? serviceUri = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Register(new AzureBlobConnectionFactory());

        return builder.UseStorage(new AzureBlobObjectStorage(accountName, container, identity, serviceUri));
    }
}
