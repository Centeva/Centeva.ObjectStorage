namespace Centeva.ObjectStorage;

public class Blob : IEquatable<Blob>
{
    public string Name { get; private set; }
    public string FullPath => StoragePath.Combine(FolderPath, Filename);
    public string FolderPath { get; private set; }
    public string Filename { get; private set; }

    public long? SizeInBytes { get; set;}
    public DateTimeOffset LastModified { get; set; }
    public DateTimeOffset Created { get; set; }

    public Blob(string objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        Name = StoragePath.Normalize(objectName, true);

        var parts = StoragePath.Split(Name);
        Filename = parts.Last();
        FolderPath = parts.Length > 1
            ? StoragePath.PathSeparator + string.Join(StoragePath.PathSeparator, parts.Take(parts.Length - 1))
            : StoragePath.RootFolderPath;
    }

    public override string ToString() => Name;

    /// <summary>
    /// Compare to another Blob instance
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if both reference the same blob</returns>
    public bool Equals(Blob? other)
    {
        if (other is null)
            return false;

        return Name == other.Name;
    }

    /// <summary>
    /// Calculate hash code
    /// </summary>
    public override int GetHashCode() => Name.GetHashCode();

    /// <summary>
    /// Compare this to any other object type
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if both reference the same blob</returns>
    public override bool Equals(object? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (other.GetType() != typeof(Blob))
            return false;

        return Equals((Blob)other);
    }

    /// <summary>
    /// Convert a string to a Blob
    /// </summary>
    /// <param name="blob"></param>
    public static implicit operator string(Blob blob) => blob.Name;

    /// <summary>
    /// Convert a Blob to a string
    /// </summary>
    /// <param name="objectName"></param>
    public static implicit operator Blob(string objectName) => new(objectName);
}
