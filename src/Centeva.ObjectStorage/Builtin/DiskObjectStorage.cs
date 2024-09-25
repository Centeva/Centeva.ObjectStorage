namespace Centeva.ObjectStorage.Builtin;

public class DiskObjectStorage : IObjectStorage
{
    private readonly string _directoryPath;

    public DiskObjectStorage(string directoryPath)
    {
        _directoryPath = Path.GetFullPath(directoryPath);
    }

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(StoragePath.Normalize(objectName), createIfMissing: false);

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

    public Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(StoragePath.Normalize(objectName), createIfMissing: false);

        return Task.FromResult(File.Exists(filePath));
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();

        if (!Directory.Exists(_directoryPath))
        {
            return Task.FromResult<IReadOnlyCollection<string>>(list);
        }

        var filenames = Directory.GetFiles(_directoryPath, "*", SearchOption.AllDirectories)
            .Select(ToObjectName);
        list.AddRange(filenames);

        return Task.FromResult<IReadOnlyCollection<string>>(list);
    }

    public Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(StoragePath.Normalize(objectName), createIfMissing: false);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(filePath);

        return Task.FromResult<Stream?>(stream);
    }

    public Task RenameAsync(string objectName, string newName, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(StoragePath.Normalize(objectName), createIfMissing: false);
        string newFilePath = GetFilePath(StoragePath.Normalize(newName), createIfMissing: true);

        if (File.Exists(filePath))
        {
            File.Move(filePath, newFilePath);
        }

        return Task.CompletedTask;
    }

    public Task WriteAsync(string objectName, Stream dataStream, string? contentType = null, CancellationToken cancellationToken = default)
    {
        string filePath = GetFilePath(StoragePath.Normalize(objectName));

        using Stream s = File.Create(filePath);
        s.Seek(0, SeekOrigin.End);
        dataStream.CopyTo(s);

        return Task.CompletedTask;
    }

    protected string GetFilePath(string objectName, bool createIfMissing = true)
    {
        objectName = objectName.Trim(StoragePath.PathSeparator);

        string[] pathParts = objectName.Split(StoragePath.PathSeparator).ToArray();
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

    private string ToObjectName(string path)
    {
        string relativePath = path.Substring(_directoryPath.Length);
        relativePath = relativePath.Replace(Path.DirectorySeparatorChar, StoragePath.PathSeparator);
        relativePath = relativePath.Trim(StoragePath.PathSeparator);

        return relativePath;
    }
}