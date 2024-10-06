namespace Centeva.ObjectStorage.UnitTests.Fixtures;

internal class TestProvider : ISignedUrlObjectStorage
{
    public Task<Stream?> OpenReadAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(StoragePath storagePath, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Uri> GetDownloadUrlAsync(StoragePath storagePath, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenameAsync(StoragePath storagePath, StoragePath newStoragePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StorageEntry?> GetAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}