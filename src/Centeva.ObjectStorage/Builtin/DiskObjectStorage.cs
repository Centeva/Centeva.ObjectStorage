using System.IO;

namespace Centeva.ObjectStorage.Builtin;

public class DiskObjectStorage : IObjectStorage
{
    private readonly string _directoryPath;

    public DiskObjectStorage(string directoryPath)
    {
        _directoryPath = Path.GetFullPath(directoryPath);
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();

        if (!Directory.Exists(_directoryPath))
        {
            return Task.FromResult<IReadOnlyCollection<string>>(list);
        }

        var filenames = Directory.GetFiles(_directoryPath, "*", SearchOption.AllDirectories)
            .Select(ToStoragePath);
        list.AddRange(filenames);

        return Task.FromResult<IReadOnlyCollection<string>>(list.AsReadOnly());
    }

    public Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, CancellationToken cancellationToken = default)
    {
        if (path is {IsFolder: false})
        {
            throw new ArgumentException("Path needs to be a folder", nameof(path));
        }

        var folderPath = GetFilePath(path, createIfMissing: false);

        if (!Directory.Exists(folderPath))
        {
            return Task.FromResult<IReadOnlyCollection<StorageEntry>>([]);
        }

        var folderInfo = new DirectoryInfo(folderPath);

        var fileInfo = folderInfo.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);

        return Task.FromResult<IReadOnlyCollection<StorageEntry>>(fileInfo.Select(ToStorageEntry).ToList());
    }

    public Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(path, createIfMissing: false);

        return Task.FromResult(File.Exists(filePath));
    }

    public Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        string localPath = GetFilePath(path, createIfMissing: false);

        bool existsAsFile = File.Exists(localPath);
        if (!existsAsFile && !Directory.Exists(localPath))
        {
            return Task.FromResult<StorageEntry?>(null);
        }

        FileSystemInfo info =
            existsAsFile ? new FileInfo(localPath) : new DirectoryInfo(localPath);

        return Task.FromResult<StorageEntry?>(ToStorageEntry(info));
    }
    public Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(path, createIfMissing: false);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(filePath);

        return Task.FromResult<Stream?>(stream);
    }

    public Task WriteAsync(StoragePath path, Stream contentStream, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(path);

        using Stream s = File.Create(filePath);
        s.Seek(0, SeekOrigin.End);
        contentStream.CopyTo(s);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(path, createIfMissing: false);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else if (Directory.Exists(filePath))
        {
            Directory.Delete(filePath, true);
        }

        return Task.CompletedTask;
    }

    public Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(sourcePath, createIfMissing: false);
        string newFilePath = GetFilePath(destinationPath, createIfMissing: true);

        if (File.Exists(filePath))
        {
            File.Move(filePath, newFilePath);
        }

        return Task.CompletedTask;
    }

    protected string GetFilePath(string storagePath, bool createIfMissing = true)
    {
        storagePath = storagePath.Trim(StoragePath.PathSeparator);

        string[] pathParts = storagePath.Split(StoragePath.PathSeparator).ToArray();
        string filename = pathParts[pathParts.Length - 1];

        string directoryPath = _directoryPath;

        if (pathParts.Length > 1)
        {
            directoryPath = Path.Combine(directoryPath, Path.Combine(pathParts.Take(pathParts.Length - 1).ToArray()));
        }

        if (createIfMissing && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return Path.Combine(directoryPath, filename);
    }

    private string ToStoragePath(string path)
    {
        string relativePath = path.Substring(_directoryPath.Length);
        relativePath = relativePath.Replace(Path.DirectorySeparatorChar, StoragePath.PathSeparator);

        return StoragePath.Normalize(relativePath);
    }

    private StorageEntry ToStorageEntry(FileSystemInfo info)
    {
        var path = ToStoragePath(info.FullName);
        bool isFolder = info is DirectoryInfo;
        if (isFolder)
        {
            path += StoragePath.PathSeparatorString;
        }

        var entry = new StorageEntry(path)
        {
            CreationTime = info.CreationTimeUtc,
            LastModificationTime = info.LastWriteTimeUtc,
        };

        if (!isFolder)
        {
            entry.SizeInBytes = ((FileInfo)info).Length;
        }

        return entry;
    }
}