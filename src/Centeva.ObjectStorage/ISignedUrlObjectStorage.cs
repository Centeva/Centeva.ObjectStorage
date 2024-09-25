namespace Centeva.ObjectStorage;

/// <summary>
/// Extended Object Storage interface that provides the ability to produce signed URLs
/// </summary>
public interface ISignedUrlObjectStorage : IObjectStorage
{

    /// <summary>
    /// Gets a signed URL for retrieving an object
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="lifetimeInSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> GetDownloadUrlAsync(StoragePath objectName, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default);
}