using Centeva.ObjectStorage.Builtin;
using Centeva.ObjectStorage.UnitTests.Fixtures;

namespace Centeva.ObjectStorage.UnitTests;
public class StorageFactoryTests
{
    [Fact]
    public void CanRegisterAndRetrieveProviders()
    {
        var factory = new StorageFactory();
        factory.Register(new TestProviderFactory());
        var connection = factory.CreateConnection("test://param=one");

        connection.Should().NotBeNull();
        connection.Should().BeOfType<TestProvider>();

    }

    [Fact]
    public void ThrowsExceptionWithUnrecognizedProvider()
    {
        var factory = new StorageFactory();

        var act = () => factory.CreateConnection("test://param=one");

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.Message.Should().Contain("test");
    }

    [Fact]
    public void RegistersBuiltinProviders()
    {
        var factory = new StorageFactory();
        var connection = factory.CreateConnection("disk://path=/tmp");

        connection.Should().NotBeNull();
        connection.Should().BeOfType<DiskObjectStorage>();
    }

    [Fact]
    [Obsolete("Verifies the obsolete GetConnection shim still works.")]
    public void GetConnectionForwardsToCreateConnection()
    {
        var factory = new StorageFactory();
        var connection = factory.GetConnection("disk://path=/tmp");

        connection.Should().NotBeNull();
        connection.Should().BeOfType<DiskObjectStorage>();
    }
}
