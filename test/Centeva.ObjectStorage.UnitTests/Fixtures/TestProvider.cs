namespace Centeva.ObjectStorage.UnitTests.Fixtures;

internal class TestProvider : ISignedUrlObjectStorage
{
    public Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(string objectName, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Blob>> ListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Uri> GetDownloadUrlAsync(string objectName, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}