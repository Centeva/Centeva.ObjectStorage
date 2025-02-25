using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.UnitTests.Connections;

public class ObjectStorageConnectionStringTests
{
    [Fact]
    public void WithBadStringFormat_ThrowsException()
    {
        var act = () => new ObjectStorageConnectionString("bad string");

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void WithValidString_ParsesProviderName()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        str.ProviderName.ShouldBe("test");
    }

    [Fact]
    public void StoresOriginalConnectionString()
    {
        const string cs = "test://bucket=mybucket";

        var str = new ObjectStorageConnectionString(cs);

        str.ConnectionString.ShouldBe(cs);
    }

    [Fact]
    public void WithValidString_ParsesParameters()
    {
        var str = new ObjectStorageConnectionString("test://user=myuser;key=somekey;bucket=mybucket");

        str.GetRequired("user").ShouldBe("myuser");
        str.GetRequired("key").ShouldBe("somekey");
        str.Get("bucket").ShouldBe("mybucket");
    }

    [Fact]
    public void GetRequired_WithMissingParameter_ThrowsException()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        var act = () => str.GetRequired("missing");

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void GetRequired_PerformsCaseInsensitiveLookup()
    {
        var str = new ObjectStorageConnectionString("test://Bucket=my_bucket");

        str.GetRequired("bucket").ShouldBe("my_bucket");
    }

    [Fact]
    public void Get_WithMissingParameter_ReturnsNull()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        str.Get("user").ShouldBeNull();
    }

    [Fact]
    public void Get_PerformsCaseInsensitiveLookup()
    {
        var str = new ObjectStorageConnectionString("test://Bucket=my_bucket");

        str.Get("bucket").ShouldBe("my_bucket");
    }

    [Theory]
    [InlineData("va=lue")]
    public void HandlesSpecialCharactersInParameters(string value)
    {
        var str = new ObjectStorageConnectionString($"test://key={value}");

        str.GetRequired("key").ShouldBe(value);
    }

    [Fact]
    public void UrlDecodesParameterValues()
    {
        var str = new ObjectStorageConnectionString("test://key=val%3Bue");

        str.GetRequired("key").ShouldBe("val;ue");
    }
}
