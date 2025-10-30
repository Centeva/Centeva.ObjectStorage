namespace Centeva.ObjectStorage;

public static class ObjectStorageExtensions
{
    /// <summary>
    /// Copies a single object from the source storage at the specified path to the target storage at the target path.
    /// </summary>
    /// <param name="sourceStorage">The source <see cref="IObjectStorage"/> instance.</param>
    /// <param name="sourcePath">The path <see cref="StoragePath"/> of the object to copy in the source storage.</param>
    /// <param name="targetStorage">The target <see cref="IObjectStorage"/> instance.</param>
    /// <param name="targetPath">
    /// The destination path <see cref="StoragePath"/> in the target storage.
    /// If <c>null</c>, the source path is used. If a folder, the source
    /// object's name is appended to the folder path.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous copy operation.</returns>
    /// <exception cref="IOException">Thrown if the source file does not exist.</exception>
    public static async Task CopyAsync(this IObjectStorage sourceStorage, StoragePath sourcePath, IObjectStorage targetStorage, StoragePath? targetPath, CancellationToken cancellationToken = default)
    {
        StoragePath destPath = targetPath?.IsFolder == true
            ? StoragePath.Combine(targetPath.Full, sourcePath.Name)
            : targetPath ?? sourcePath;

        StorageEntry? entry = await sourceStorage.GetAsync(sourcePath, cancellationToken);
        WriteOptions? writeOptions = null;
        if (entry != null)
            writeOptions = new WriteOptions(entry.ContentType, entry.Metadata);

        using var sourceStream = await sourceStorage.OpenReadAsync(sourcePath, cancellationToken)
            ?? throw new IOException($"Source file not found: {sourcePath}");
        await targetStorage.WriteAsync(destPath, sourceStream!, writeOptions, cancellationToken);
    }

    /// <summary>
    /// Recursively copies all objects from the source storage under the specified path to the target storage at the target path.
    /// </summary>
    /// <param name="sourceStorage">The source <see cref="IObjectStorage"/> instance.</param>
    /// <param name="sourcePath">The root path <see cref="StoragePath"/> in the source storage to copy from.</param>
    /// <param name="targetStorage">The target <see cref="IObjectStorage"/> instance.</param>
    /// <param name="targetPath">
    /// The root destination path <see cref="StoragePath"/> in the target storage.
    /// If a folder, the relative structure is preserved under this folder.
    /// If <c>null</c>, the relative structure is preserved from the source path.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous recursive copy operation.</returns>
    /// <exception cref="IOException">Thrown if a source file does not exist during the copy process.</exception>
    public static async Task CopyAllAsync(this IObjectStorage sourceStorage, StoragePath sourcePath, IObjectStorage targetStorage, StoragePath? targetPath, CancellationToken cancellationToken = default)
    {
        var entries = await sourceStorage.ListAsync(sourcePath, new ListOptions { Recurse = true }, cancellationToken);
        var entryPaths = entries.Where(x => !x.Path.IsFolder).Select(x => x.Path).ToList();

        foreach (var entryPath in entryPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = entryPath.Full.Substring(sourcePath.Full.Length);
            StoragePath targetEntryPath = targetPath?.IsFolder == true
                ? StoragePath.Combine(targetPath.Full, relativePath)
                : relativePath;

            await sourceStorage.CopyAsync(entryPath, targetStorage, targetEntryPath, cancellationToken);
        }
    }
}
