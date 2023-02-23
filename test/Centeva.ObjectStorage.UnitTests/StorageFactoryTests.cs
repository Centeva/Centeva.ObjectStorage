using Centeva.ObjectStorage.GCP;
using Centeva.ObjectStorage.S3;

namespace Centeva.ObjectStorage.UnitTests;
public class ConnectionFactoryTests
{
    [Fact]
    public void CanRegisterProviders()
    {
        var connection = new StorageFactory()
            .UseGoogleCloudStorage()
            .UseS3CompatibleStorage()
            .GetConnection("");
    }
}
