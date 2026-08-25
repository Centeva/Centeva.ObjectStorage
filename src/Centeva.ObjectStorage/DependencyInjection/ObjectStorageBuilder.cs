using Microsoft.Extensions.DependencyInjection;

namespace Centeva.ObjectStorage.DependencyInjection;

/// <summary>
/// Configures object storage for registration with a dependency injection
/// container.  Inherits from <see cref="StorageFactory"/> so that all existing
/// provider registration extension methods (for example
/// <c>UseAwsS3Storage()</c>) can be used here as well.
/// </summary>
public class ObjectStorageBuilder : StorageFactory
{
    private string? _connectionString;
    private IObjectStorage? _storage;

    internal ObjectStorageBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// The service collection the object storage services are registered with.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Create the <see cref="IObjectStorage"/> instance from the given
    /// connection string, using the registered storage providers.
    /// </summary>
    public ObjectStorageBuilder UseConnectionString(string connectionString)
    {
        if (connectionString is null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
        _storage = null;

        return this;
    }

    /// <summary>
    /// Use an already-constructed <see cref="IObjectStorage"/> instance.  This
    /// is typically called by provider-specific extension methods.
    /// </summary>
    public ObjectStorageBuilder UseStorage(IObjectStorage storage)
    {
        if (storage is null)
        {
            throw new ArgumentNullException(nameof(storage));
        }

        _storage = storage;
        _connectionString = null;

        return this;
    }

    internal IObjectStorage Build()
    {
        if (_storage is not null)
        {
            return _storage;
        }

        if (_connectionString is not null)
        {
            return GetConnection(_connectionString);
        }

        throw new InvalidOperationException(
            "No object storage was configured.  Call UseConnectionString() or a provider-specific configuration method.");
    }
}
