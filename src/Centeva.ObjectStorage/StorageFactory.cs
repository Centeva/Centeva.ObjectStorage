using Centeva.ObjectStorage.Builtin;
using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage;

public class StorageFactory
{
    private readonly List<IConnectionFactory> _providerFactories = [
        new BuiltinConnectionFactory(),
    ];

    public void Register(IConnectionFactory factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _providerFactories.Add(factory);
    }

    public IObjectStorage GetConnection(string connectionString)
    {
        if (connectionString is null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var cs = new ObjectStorageConnectionString(connectionString);

        IObjectStorage? storage = _providerFactories
            .Select(f => f.CreateConnection(cs))
            .FirstOrDefault(f => f != null);

        return storage ?? throw new ArgumentException(
            $"Could not find a storage provider based on the given connection string (provider: {cs.ProviderName})",
            nameof(connectionString));
    }
}
