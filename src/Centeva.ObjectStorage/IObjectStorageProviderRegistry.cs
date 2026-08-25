using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage;

/// <summary>
/// Registers the storage providers a <see cref="StorageFactory"/> (or a
/// subclass, such as
/// <see cref="Centeva.ObjectStorage.DependencyInjection.ObjectStorageBuilder"/>)
/// uses to resolve connection strings.  Provider packages target this
/// interface -- rather than <see cref="StorageFactory"/> directly -- so their
/// registration extension methods (for example <c>UseAwsS3Storage()</c>) stay
/// generic over the caller's actual type and don't lose access to
/// subclass-only members like <c>UseConnectionString</c> when chained.
/// </summary>
public interface IObjectStorageProviderRegistry
{
    /// <summary>
    /// Register a storage provider.
    /// </summary>
    void Register(IConnectionFactory factory);
}
