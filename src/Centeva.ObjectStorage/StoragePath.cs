namespace Centeva.ObjectStorage;

public sealed class StoragePath : IEquatable<StoragePath>
{
    /// <summary>
    /// Character used to split paths
    /// </summary>
    public const char PathSeparator = '/';

    /// <summary>
    /// Character used to split paths, as a string
    /// </summary>
    public static readonly string PathSeparatorString = new(PathSeparator, 1);

    /// <summary>
    /// Path for root of storage
    /// </summary>
    public static readonly string RootFolderPath = "/";

    /// <summary>
    /// Folder name for going up the path
    /// </summary>
    private const string UpFolderName = "..";

    private readonly string _path;
    private readonly string _name;
    private readonly string _folder;
    private string? _pathWithoutLeadingSlash;

    public string Full => _path;

    public string Name => _name;

    public string Folder => _folder;

    public bool IsFolder => _name.EndsWith(PathSeparatorString);

    public bool IsFile => !IsFolder;

    public StoragePath(string path)
    {
        _path = Normalize(path);

        if (IsRootPath(path))
        {
            _name = RootFolderPath;
            _folder = RootFolderPath;
        }
        else
        {
            var parts = Split(path);

            _name = parts.Length == 0 ? RootFolderPath : parts[parts.Length - 1];
            _folder = GetParent(path) ?? RootFolderPath;
        }
    }

    public override string ToString() => Full;

    /// <summary>
    /// Constructs a StoragePath from a string by implicit conversion
    /// </summary>
    public static implicit operator StoragePath(string? path)
        => path is null ? new StoragePath(RootFolderPath) : new StoragePath(path);

    /// <summary>
    /// Converts a StoragePath to a string by using full path, as an implicit conversion
    /// </summary>
    /// <param name="path"></param>
    public static implicit operator string(StoragePath? path)
        => path?.Full ?? RootFolderPath;

    public string WithoutLeadingSlash => _pathWithoutLeadingSlash ??= Normalize(_path, true);

    /// <summary>
    /// Normalize a storage path.  Removes path separators from end and collapses parent references.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="removeLeadingSlash"></param>
    /// <returns></returns>
    public static string Normalize(string path, bool removeLeadingSlash = false)
    {
        if (IsRootPath(path))
        {
            return RootFolderPath;
        }

        var parts = Split(path);

        var normalizedParts = new List<string>(parts.Length);
        foreach (string part in parts)
        {
            if (part == UpFolderName || part == $"{UpFolderName}{PathSeparatorString}")
            {
                if (normalizedParts.Count > 0)
                {
                    normalizedParts.RemoveAt(normalizedParts.Count - 1);
                }
            }
            else
            {
                normalizedParts.Add(part);
            }
        }

        path = string.Join(PathSeparatorString, normalizedParts);

        var normal = removeLeadingSlash ?
            path :
            PathSeparator + path;

        return IsRootPath(normal)
            ? RootFolderPath
            : normal;
    }

    /// <summary>
    /// Get the parent path for the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? GetParent(string? path)
    {
        if (path is null)
        {
            return null;
        }

        path = Normalize(path);

        var parts = Split(path);

        if (parts.Length == 0)
        {
            return null;
        }

        return parts.Length == 1
            ? PathSeparatorString
            : Combine(parts.Take(parts.Length - 1)) + PathSeparatorString;
    }

    /// <summary>
    /// Combine parts of a storage path into one string with separators
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string Combine(IEnumerable<string> parts)
    {
        return Normalize(string.Join(PathSeparatorString, parts.Select(NormalizePathPart)));
    }

    /// <summary>
    /// Combine parts of a storage path into one string with separators
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string Combine(params string[] parts)
    {
        return Combine((IEnumerable<string>)parts);
    }

    /// <summary>
    /// Checks if a storage path is the root folder path, which can be an empty string, null, or the actual root path.
    /// </summary>
    public static bool IsRootPath(string path)
    {
        return string.IsNullOrEmpty(path) || path == RootFolderPath;
    }

    /// <summary>
    /// Splits a storage path into parts.  Does not resolve "up" path parts or
    /// remove trailing separators.
    /// </summary>
    public static string[] Split(string path)
    {
        bool isFolder = path.EndsWith(PathSeparatorString);

        var parts = path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizePathPart)
            .ToArray();

        if (isFolder && parts.Length > 0)
        {
            parts[parts.Length - 1] = parts[parts.Length - 1] + PathSeparatorString;
        }

        return parts;
    }

    private static string NormalizePathPart(string part)
    {
        return part.Trim(PathSeparator);
    }

    public bool Equals(StoragePath? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _path == other._path;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is StoragePath other && Equals(other);
    }

    public override int GetHashCode() => _path.GetHashCode();
}
