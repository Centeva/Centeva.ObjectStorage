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
        var connection = factory.GetConnection("test://param=one");

        connection.ShouldNotBeNull();
        connection.ShouldBeOfType<TestProvider>();

    }

    [Fact]
    public void ThrowsExceptionWithUnrecognizedProvider()
    {
        var factory = new StorageFactory();

        var act = () => factory.GetConnection("test://param=one");

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("test");
    }

    [Fact]
    public void RegistersBuiltinProviders()
    {
        var factory = new StorageFactory();
        var connection = factory.GetConnection("disk://path=/tmp");

        connection.ShouldNotBeNull();
        connection.ShouldBeOfType<DiskObjectStorage>();
    }
}
