namespace Centeva.ObjectStorage;

public static class ObjectStorageExtensions
{
    public static async Task CopyAsync(this IObjectStorage sourceStorage, StoragePath sourcePath, IObjectStorage targetStorage, StoragePath? targetPath, CancellationToken cancellationToken = default)
    {
        var destPath = targetPath?.IsFolder == true
            ? new StoragePath(StoragePath.Combine(targetPath.Full, sourcePath.Name))
            : targetPath ?? sourcePath;

        using var sourceStream = await sourceStorage.OpenReadAsync(sourcePath, cancellationToken)
            ?? throw new IOException($"Source file not found: {sourcePath}");
        await targetStorage.WriteAsync(destPath, sourceStream!, cancellationToken: cancellationToken);
    }

    public static async Task CopyAllAsync(this IObjectStorage sourceStorage, StoragePath sourcePath, IObjectStorage targetStorage, StoragePath? targetPath, CancellationToken cancellationToken = default)
    {
        var entries = await sourceStorage.ListAsync(sourcePath, new ListOptions { Recurse = true }, cancellationToken);
        var entryPaths = entries.Where(x => !x.Path.IsFolder).Select(x => x.Path).ToList();

        foreach (var entryPath in entryPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = entryPath.Full.Substring(sourcePath.Full.Length);
            var targetEntryPath = targetPath?.IsFolder == true
                ? new StoragePath(StoragePath.Combine(targetPath.Full, relativePath))
                : new StoragePath(relativePath);

            await sourceStorage.CopyAsync(entryPath, targetStorage, targetEntryPath, cancellationToken);
        }
    }
}
