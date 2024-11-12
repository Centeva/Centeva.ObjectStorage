namespace Centeva.ObjectStorage;

/// <summary>
/// Marker interface to denote that a storage provider currently supports metadata for storage entries.
/// </summary>
public interface ISupportsMetadata
{

    /// <summary>
    /// Update information about a stored object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateMetadataAsync(StoragePath path, UpdateStorageEntryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get metadata for a stored object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageEntry> GetMetadataAsync(StoragePath path, CancellationToken cancellationToken = default);
}
