﻿namespace Centeva.ObjectStorage.UnitTests.Fixtures;

internal class TestProvider : ISignedUrlObjectStorage
{
    public Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, bool recurse = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(StoragePath path, Stream contentStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}