namespace Centeva.ObjectStorage;

public interface IObjectStorage
{
    Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default);

    Task WriteAsync(string objectName, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetExistingAsync(int pageSize, CancellationToken cancellationToken = default);

    bool SupportsSignedUrls { get; }

    Task<Uri> GetSignedUrlAsync(string objectName, TimeSpan lifetime, CancellationToken cancellationToken = default);
}
