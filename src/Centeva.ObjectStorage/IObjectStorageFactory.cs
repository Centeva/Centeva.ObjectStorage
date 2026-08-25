namespace Centeva.ObjectStorage;

/// <summary>
/// Resolves <see cref="IObjectStorage"/> connections from connection strings
/// at runtime, using the registered storage providers.  Register this with
/// the dependency injection container via
/// <c>IServiceCollection.AddObjectStorageFactory()</c> when the connection
/// string isn't known until runtime -- for example, in a multi-tenant
/// application where each tenant has its own storage configuration.  For the
/// common case of a single, statically-configured storage instance, use
/// <c>AddObjectStorage</c> instead.
/// </summary>
public interface IObjectStorageFactory
{
    /// <summary>
    /// Create an <see cref="IObjectStorage"/> instance from the given
    /// connection string, using the registered storage providers.
    /// </summary>
    IObjectStorage CreateConnection(string connectionString);
}
