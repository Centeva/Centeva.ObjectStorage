using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.UnitTests;

public class ObjectStorageConnectionStringTests
{
    [Fact]
    public void Get_DecodesUrlEncodedParameters()
    {
        var cs = new ObjectStorageConnectionString("provider://key=hello%20world");

        cs.Get("key").ShouldBe("hello world");
    }

    [Fact]
    public void Get_DoesNotDecodePlusAsSpace()
    {
        var cs = new ObjectStorageConnectionString("provider://key=abc+def");

        cs.Get("key").ShouldBe("abc+def");
    }
}
