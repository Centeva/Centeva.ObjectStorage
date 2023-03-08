namespace Centeva.ObjectStorage.Connections;

public interface IConnectionFactory
{
    IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString);
}
