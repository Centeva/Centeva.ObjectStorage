namespace Centeva.ObjectStorage;

/// <summary>
/// Interface for working with object storage.  Platform-specific providers will implement this.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Returns the list of available stored objects
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of object names</returns>
    Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an object exists in storage
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the object exists</returns>
    Task<bool> ExistsAsync(StoragePath storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Open a stream for reading the stored object with the given name
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream?> OpenReadAsync(StoragePath storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about a stored object at the given path
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageEntry?> GetAsync(StoragePath storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write to a stored object with the given name via a stream
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="dataStream"></param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(StoragePath storagePath, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the stored object with the given name
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(StoragePath storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a stored object
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RenameAsync(StoragePath storagePath, StoragePath newName, CancellationToken cancellationToken = default);
}
