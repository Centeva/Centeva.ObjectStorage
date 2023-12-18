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

        connection.Should().NotBeNull();
        connection.Should().BeOfType<TestProvider>();

    }

    [Fact]
    public void ThrowsExceptionWithUnrecognizedProvider()
    {
        var factory = new StorageFactory();

        var act = () => factory.GetConnection("test://param=one");

        act.Should().Throw<ArgumentException>().WithMessage("*test*");
    }

    [Fact]
    public void RegistersBuiltinProviders()
    {
        var factory = new StorageFactory();
        var connection = factory.GetConnection("disk://path=/tmp");

        connection.Should().NotBeNull();
        connection.Should().BeOfType<DiskObjectStorage>();
    }
}
