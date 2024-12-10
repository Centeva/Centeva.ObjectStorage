namespace Centeva.ObjectStorage;

public sealed class StorageListOptions {

    /// <summary>
    /// Path to list within.  If <see langword="null">null</see>, uses the root path of the storage provider.
    /// </summary>
    public StoragePath? Path { get; set; } = null;

    /// <summary>
    /// Set to true to recurse inside "folders"
    /// </summary>
    public bool Recurse { get; set; } = false;

    /// <summary>
    /// Set to true to include metadata in the results
    /// </summary>
    public bool IncludeMetadata { get; set; } = false;
}
