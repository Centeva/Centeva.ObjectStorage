using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Builtin;

public class BuiltinConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "disk";
    private const string Path = "path";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName == ProviderName)
        {
            var path = connectionString.GetRequired(Path);

            return new DiskObjectStorage(path);
        }

        return null;
    }
}
