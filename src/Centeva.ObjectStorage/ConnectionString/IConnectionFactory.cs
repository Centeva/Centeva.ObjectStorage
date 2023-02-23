namespace Centeva.ObjectStorage.ConnectionString;

public interface IConnectionFactory
{
    IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString);
}
