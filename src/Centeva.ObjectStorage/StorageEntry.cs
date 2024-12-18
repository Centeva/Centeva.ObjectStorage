namespace Centeva.ObjectStorage;
public class StorageEntry
{
    /// <summary>
    /// Path of this entry within the context of the storage provider
    /// </summary>
    public StoragePath Path { get; internal set; } = null!;

    /// <summary>
    /// Name of the entry, which is the last part of the path
    /// </summary>
    public string Name => Path.Name;

    /// <summary>
    /// Time of creation of this entry, if known
    /// </summary>
    public DateTimeOffset? CreationTime { get; set; }

    /// <summary>
    /// Time of last modification of this entry, if known
    /// </summary>
    public DateTimeOffset? LastModificationTime { get; set; }

    /// <summary>
    /// Size of this entry in bytes, if known
    /// </summary>
    public long? SizeInBytes { get; set; }

    /// <summary>
    /// Content type of this entry, if known
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// User-defined metadata, as key/value pairs
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; set; }

    public StorageEntry(string path)
    {
        SetPath(path);
    }

    public void SetPath(string path)
    {
        Path = path;
    }
}
