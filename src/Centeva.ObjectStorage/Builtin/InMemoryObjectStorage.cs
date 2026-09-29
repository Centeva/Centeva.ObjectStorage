using System.Collections.ObjectModel;
using System.IO;

namespace Centeva.ObjectStorage.Builtin;

/// <summary>
/// Object storage provider that keeps all objects in process memory.  Nothing
/// is persisted, and each instance is an independent, initially empty store.
/// Intended for functional and integration tests, or local development, where
/// connecting to a real filesystem or cloud service is not wanted.
/// </summary>
/// <remarks>
/// Like the cloud blob providers, this is a flat keyspace: "folders" are
/// implied by the "/" separators in object paths and are never stored on their
/// own.  All members are safe to call concurrently.
/// </remarks>
public class InMemoryObjectStorage : IObjectStorage, ISupportsMetadata
{
    private readonly Dictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, ListOptions options = default, CancellationToken cancellationToken = default)
    {
        if (path is { IsFolder: false })
        {
            throw new ArgumentException("Path needs to be a folder", nameof(path));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var prefix = path?.Full ?? StoragePath.RootFolderPath;
        var entries = new List<StorageEntry>();
        var folders = new HashSet<string>(StringComparer.Ordinal);

        lock (_lock)
        {
            foreach (var pair in _objects.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                if (pair.Key.Length <= prefix.Length || !pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var separatorIndex = pair.Key.IndexOf(StoragePath.PathSeparator, prefix.Length);

                if (options.Recurse || separatorIndex < 0)
                {
                    entries.Add(pair.Value.ToStorageEntry(pair.Key, options.IncludeMetadata));
                }
                else
                {
                    // Collapse everything below the next separator into a
                    // single folder entry, as a delimited blob listing would.
                    var folder = pair.Key.Substring(0, separatorIndex + 1);
                    if (folders.Add(folder))
                    {
                        entries.Add(new StorageEntry(folder));
                    }
                }
            }
        }

        if (options.Recurse)
        {
            entries.InsertRange(0, FolderHelper.GetImpliedFolders(entries, path));
        }

        return Task.FromResult<IReadOnlyCollection<StorageEntry>>(entries.AsReadOnly());
    }

    public Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            return Task.FromResult(_objects.ContainsKey(path.Full));
        }
    }

    public Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            return Task.FromResult(_objects.TryGetValue(path.Full, out var stored)
                ? stored.ToStorageEntry(path.Full, includeMetadata: true)
                : null);
        }
    }

    public Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            // Stored content is never modified in place (a write replaces the
            // whole array), so a read-only view of it is safe to hand out.
            return Task.FromResult<Stream?>(_objects.TryGetValue(path.Full, out var stored)
                ? new MemoryStream(stored.Content, writable: false)
                : null);
        }
    }

    public async Task WriteAsync(StoragePath path, Stream contentStream, WriteOptions? writeOptions = null, CancellationToken cancellationToken = default)
    {
        if (contentStream is null)
        {
            throw new ArgumentNullException(nameof(contentStream));
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var buffer = new MemoryStream();
        await contentStream.CopyToAsync(buffer, 81920, cancellationToken).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        var stored = new StoredObject(buffer.ToArray(), now, now, writeOptions?.ContentType, CopyMetadata(writeOptions?.Metadata));

        lock (_lock)
        {
            _objects[path.Full] = stored;
        }
    }

    public Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _objects.Remove(path.Full);
        }

        return Task.CompletedTask;
    }

    public Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (sourcePath.Full != destinationPath.Full && _objects.TryGetValue(sourcePath.Full, out var stored))
            {
                _objects[destinationPath.Full] = stored;
                _objects.Remove(sourcePath.Full);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Replace the metadata of the object at the given path.
    /// </summary>
    /// <exception cref="FileNotFoundException">No object exists at the given path.</exception>
    public Task UpdateMetadataAsync(StoragePath path, UpdateStorageEntryRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (!_objects.TryGetValue(path.Full, out var stored))
            {
                throw new FileNotFoundException($"Object not found: {path}");
            }

            _objects[path.Full] = stored.WithMetadata(CopyMetadata(request.Metadata), DateTimeOffset.UtcNow);
        }

        return Task.CompletedTask;
    }

    private static IReadOnlyDictionary<string, string> CopyMetadata(IEnumerable<KeyValuePair<string, string>>? metadata)
    {
        var copy = new Dictionary<string, string>();

        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                copy[pair.Key] = pair.Value;
            }
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }

    private sealed class StoredObject
    {
        public StoredObject(byte[] content, DateTimeOffset creationTime, DateTimeOffset lastModificationTime, string? contentType, IReadOnlyDictionary<string, string> metadata)
        {
            Content = content;
            CreationTime = creationTime;
            LastModificationTime = lastModificationTime;
            ContentType = contentType;
            Metadata = metadata;
        }

        public byte[] Content { get; }
        public DateTimeOffset CreationTime { get; }
        public DateTimeOffset LastModificationTime { get; }
        public string? ContentType { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public StoredObject WithMetadata(IReadOnlyDictionary<string, string> metadata, DateTimeOffset lastModificationTime)
            => new(Content, CreationTime, lastModificationTime, ContentType, metadata);

        public StorageEntry ToStorageEntry(string path, bool includeMetadata)
        {
            return new StorageEntry(path)
            {
                CreationTime = CreationTime,
                LastModificationTime = LastModificationTime,
                SizeInBytes = Content.LongLength,
                ContentType = ContentType,
                Metadata = includeMetadata ? Metadata : null,
            };
        }
    }
}
