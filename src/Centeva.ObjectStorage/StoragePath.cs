namespace Centeva.ObjectStorage;

public static class StoragePath
{
    /// <summary>
    /// Character used to split paths
    /// </summary>
    public const char PathSeparator = '/';

    /// <summary>
    /// Path for root of storage
    /// </summary>
    public static readonly string RootFolderPath = "/";

    /// <summary>
    /// Folder name for going up the path
    /// </summary>
    private const string ParentFolderName = "..";

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
            if (part == ParentFolderName)
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

        path = string.Join(PathSeparator, normalizedParts);

        return removeLeadingSlash ?
            path :
            PathSeparator + path;
    }

    /// <summary>
    /// Combine parts of a storage path into one string with separators
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string Combine(IEnumerable<string> parts)
    {
        return Normalize(string.Join(PathSeparator, parts.Select(NormalizePathPart)));
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
    private static bool IsRootPath(string path)
    {
        return string.IsNullOrEmpty(path) || path == RootFolderPath;
    }

    public static string[] Split(string path)
    {
        return path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizePathPart)
            .ToArray();
    }

    private static string NormalizePathPart(string part)
    {
        return part.Trim(PathSeparator);
    }
}
