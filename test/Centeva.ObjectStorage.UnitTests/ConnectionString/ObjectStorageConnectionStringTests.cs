﻿using Centeva.ObjectStorage.ConnectionString;

namespace Centeva.ObjectStorage.UnitTests.ConnectionString;

public class ObjectStorageConnectionStringTests
{
    [Fact]
    public void WithBadStringFormat_ThrowsException()
    {
        var act = () => new ObjectStorageConnectionString("bad string");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithValidString_ParsesProviderName()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        str.ProviderName.Should().Be("test");
    }

    [Fact]
    public void StoresOriginalConnectionString()
    {
        const string cs = "test://bucket=mybucket";

        var str = new ObjectStorageConnectionString(cs);

        str.ConnectionString.Should().Be(cs);
    }

    [Fact]
    public void WithValidString_ParsesParameters()
    {
        var str = new ObjectStorageConnectionString("test://user=myuser;key=somekey;bucket=mybucket");

        str.GetRequired("user").Should().Be("myuser");
        str.GetRequired("key").Should().Be("somekey");
        str.Get("bucket").Should().Be("mybucket");
    }

    [Fact]
    public void GetRequired_WithMissingParameter_ThrowsException()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        var act = () => str.GetRequired("missing");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Get_WithMissingParameter_ReturnsNull()
    {
        var str = new ObjectStorageConnectionString("test://bucket=mybucket");

        str.Get("user").Should().BeNull();
    }


    [Theory]
    [InlineData("va=lue")]
    public void HandlesSpecialCharactersInParameters(string value)
    {
        var str = new ObjectStorageConnectionString($"test://key={value}");

        str.GetRequired("key").Should().Be(value);
    }

    [Fact]
    public void UrlDecodesParameterValues()
    {
        var str = new ObjectStorageConnectionString("test://key=val%3Bue");

        str.GetRequired("key").Should().Be("val;ue");
    }
}
