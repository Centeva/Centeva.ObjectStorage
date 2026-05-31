using Centeva.ObjectStorage.Builtin;
using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.UnitTests;

public class BuiltinConnectionFactoryTests
{
    [Fact]
    public void CreatesDiskStorage()
    {
        var factory = new BuiltinConnectionFactory();
        var connection = factory.CreateConnection(new ObjectStorageConnectionString("disk://path=/tmp"));

        connection.Should().NotBeNull();
        connection.Should().BeOfType<DiskObjectStorage>();
    }

    [Fact]
    public void ThrowsExceptionWithMissingPath()
    {
        var factory = new BuiltinConnectionFactory();

        var act = () => factory.CreateConnection(new ObjectStorageConnectionString("disk://"));
        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.Message.Should().Contain("path");
    }
}
