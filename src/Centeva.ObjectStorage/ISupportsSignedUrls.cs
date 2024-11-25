namespace Centeva.ObjectStorage;

/// <summary>
/// Extended Object Storage interface that provides the ability to produce signed URLs
/// </summary>
public interface ISupportsSignedUrls
{
    /// <summary>
    /// Gets a signed URL for retrieving an object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="lifetimeInSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a signed URL for retrieving an object at the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="lifetimeInSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> GetUploadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default);
}