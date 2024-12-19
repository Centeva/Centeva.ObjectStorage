namespace Centeva.ObjectStorage;

/// <summary>
/// Interface for working with object storage.  Platform-specific providers will implement this.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Lists entries within the specified path.
    /// </summary>
    /// <param name="path">Path to list within.  If <see langword="null">null</see>, uses the root path of the storage provider.</param>
    /// <param name="options">Options for producing the listing</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of entries stored within this path, including folders if applicable.  (Folder names end in "/".)</returns>
    Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, ListOptions options = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an object exists at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the object exists</returns>
    Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about a stored object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Open a stream for reading the stored object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write to a stored object at the given path via a stream
    /// </summary>
    /// <param name="path"></param>
    /// <param name="contentStream"></param>
    /// <param name="writeOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(StoragePath path, Stream contentStream, WriteOptions? writeOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the stored object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a stored object
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default);
}