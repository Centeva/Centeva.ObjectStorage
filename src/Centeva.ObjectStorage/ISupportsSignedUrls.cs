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
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> GetDownloadUrlAsync(StoragePath path, SignedUrlOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// (deprecated) Gets a signed URL for retrieving an object at the given path
    /// </summary>
    /// <remarks>This version of the method is deprecated in favor of passing <see cref="SignedUrlOptions"/></remarks>
    /// <param name="path"></param>
    /// <param name="lifetimeInSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default);
}