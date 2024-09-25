namespace Centeva.ObjectStorage.UnitTests.Fixtures;

internal class TestProvider : ISignedUrlObjectStorage
{
    public Task<Stream?> OpenReadAsync(StoragePath objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(StoragePath objectName, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(StoragePath objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(StoragePath objectName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Uri> GetDownloadUrlAsync(StoragePath objectName, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenameAsync(StoragePath objectName, StoragePath newName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}