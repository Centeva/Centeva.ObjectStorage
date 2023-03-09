namespace Centeva.ObjectStorage.UnitTests.Fixtures;

internal class TestProvider : IObjectStorage
{
    public Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(string objectName, Stream dataStream, CancellationToken cancellationToken = default)
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

    public Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool SupportsSignedUrls => true;

    public Task<Uri> GetDownloadUrlAsync(string objectName, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}