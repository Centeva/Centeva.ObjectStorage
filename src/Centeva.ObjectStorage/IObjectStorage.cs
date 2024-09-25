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
    /// <param name="objectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the object exists</returns>
    Task<bool> ExistsAsync(StoragePath objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Open a stream for reading the stored object with the given name
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream?> OpenReadAsync(StoragePath objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write to a stored object with the given name via a stream
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="dataStream"></param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(StoragePath objectName, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the stored object with the given name
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(StoragePath objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a stored object
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RenameAsync(StoragePath objectName, StoragePath newName, CancellationToken cancellationToken = default);
}
