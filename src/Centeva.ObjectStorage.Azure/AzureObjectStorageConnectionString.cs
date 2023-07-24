using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureObjectStorageConnectionString : ObjectStorageConnectionString
{
    public AzureObjectStorageConnectionString(string connectionString) : base(connectionString)
    {
    }

    protected override void Parse(string connectionString)
    {
        ParseParameters(connectionString, false);
    }
}
