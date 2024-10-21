namespace Centeva.ObjectStorage;
public static class FolderHelper
{
    public static List<StorageEntry> GetImpliedFolders(List<StorageEntry> entries, string? basePath)
    {
        basePath = StoragePath.Normalize(basePath);

        return entries
            .Select(x => x.Path.Full)
            .Select(x => x.Substring(basePath.Length))
            .Select(StoragePath.GetParent)
            .Where(x => !StoragePath.IsRootPath(x))
            .SelectMany(GetFolderHierarchy!)
            .Distinct()
            .Select(x => new StorageEntry(basePath + "/" + x + "/"))
            .ToList();
    }

    private static IEnumerable<string> GetFolderHierarchy(string path)
    {
        var parts = StoragePath.Split(path);

        var currentPath = "";
        foreach (var part in parts)
        {
            currentPath = StoragePath.Combine(currentPath, part);
            if (!StoragePath.IsRootPath(currentPath))
            {
                yield return currentPath + "/";
            }
        }
    }
}
