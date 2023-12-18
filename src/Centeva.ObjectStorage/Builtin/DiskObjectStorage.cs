namespace Centeva.ObjectStorage.Builtin
{
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

        public Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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

        public Task WriteAsync(string objectName, Stream dataStream, string? contentType = null, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(StoragePath.Normalize(objectName));

            using Stream s = File.Create(filePath);
            s.Seek(0, SeekOrigin.End);
            dataStream.CopyTo(s);

            return Task.CompletedTask;
        }

        private string GetFilePath(string objectName, bool createIfMissing = true)
        {
            objectName = objectName.Trim(StoragePath.PathSeparator);

            string[] pathParts = objectName.Split(StoragePath.PathSeparator).ToArray();
            string filename = pathParts[^1];

            string directoryPath = _directoryPath;

            if (pathParts.Length > 1)
            {
                directoryPath = Path.Combine(directoryPath, Path.Combine(pathParts[..^1]));
            }

            if (createIfMissing && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, filename);
        }
    }
}
