using Centeva.ObjectStorage.Builtin;
using Centeva.ObjectStorage.Connections;

using Shouldly;

namespace Centeva.ObjectStorage.UnitTests;

public class BuiltinConnectionFactoryTests
{
    [Fact]
    public void CreatesDiskStorage()
    {
        var factory = new BuiltinConnectionFactory();
        var connection = factory.CreateConnection(new ObjectStorageConnectionString("disk://path=/tmp"));

        connection.ShouldNotBeNull();
        connection.ShouldBeOfType<DiskObjectStorage>();
    }

    [Fact]
    public void ThrowsExceptionWithMissingPath()
    {
        var factory = new BuiltinConnectionFactory();

        var act = () => factory.CreateConnection(new ObjectStorageConnectionString("disk://"));
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("path");
    }
}