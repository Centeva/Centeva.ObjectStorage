using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.UnitTests.Fixtures
{
    internal class TestProviderFactory : IConnectionFactory
    {
        public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString) =>
            connectionString.ProviderName == "test" ? new TestProvider() : null;
    }
}
