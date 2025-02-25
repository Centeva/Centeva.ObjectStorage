namespace Centeva.ObjectStorage.UnitTests;

public class StoragePathTests
{
    [Theory]
    [InlineData("/", "/")]
    [InlineData("", "/")]
    [InlineData("//", "/")]
    [InlineData("dev", "/dev")]
    [InlineData("dev/", "/dev/")]
    [InlineData("/dev", "/dev")]
    [InlineData("dev/one", "/dev/one")]
    public void Constructor_NormalizesFullPath(string input, string expected)
    {
        new StoragePath(input).Full.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("dev/", "dev/")]
    [InlineData("/dev/one", "one")]
    [InlineData("dev/one/two/", "two/")]
    public void Name_ReturnsFileOrFolderName(string path, string expected)
    {
        new StoragePath(path).Name.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("dev/", "/")]
    [InlineData("/dev/one", "/dev/")]
    [InlineData("dev/one/two/", "/dev/one/")]
    [InlineData("dev/one/../two/three/..", "/dev/")]
    public void Folder(string path, string expected)
    {
        new StoragePath(path).Folder.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("dev/", true)]
    [InlineData("/dev/one", false)]
    [InlineData("dev/one/two/", true)]
    public void IsFolder(string path, bool expected)
    {
        new StoragePath(path).IsFolder.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/", false)]
    [InlineData("dev/", false)]
    [InlineData("/dev/one", true)]
    [InlineData("dev/one/two/", false)]
    public void IsFile(string path, bool expected)
    {
        new StoragePath(path).IsFile.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/dev/", "dev/")]
    [InlineData("/", "/")]
    [InlineData("dev/one/", "dev/one/")]
    public void WithoutLeadingSlash(string path, string expected)
    {
        new StoragePath(path).WithoutLeadingSlash.ShouldBe(expected);
    }

    [Theory]
    [InlineData("dev/..", "/")]
    [InlineData("dev/../storage", "/storage")]
    [InlineData("/one", "/one")]
    [InlineData("/one/", "/one/")]
    [InlineData("/one/../../../..", "/")]
    [InlineData("/one/../../../../", "/")]
    public void Normalize(string path, string normalizedPath)
    {
        StoragePath.Normalize(path).ShouldBe(normalizedPath);
    }

    [Theory]
    [InlineData("dev/..", "/")]
    [InlineData("dev/../storage", "storage")]
    [InlineData("/one", "one")]
    [InlineData("/one/", "one/")]
    [InlineData("/one/../../../..", "/")]
    [InlineData("/one/../../../../", "/")]
    public void NormalizeRemoveLeadingSlash(string path, string normalizedPath)
    {
        StoragePath.Normalize(path, true).ShouldBe(normalizedPath);
    }

    [Theory]
    [InlineData(new[] { "dev", "one" }, "/dev/one")]
    [InlineData(new[] { "one", "two", "three" }, "/one/two/three")]
    [InlineData(new[] { "one", "..", "three" }, "/three")]
    public void Combine(string[] parts, string expectedPath)
    {
        StoragePath.Combine(parts).ShouldBe(expectedPath);
    }

    [Theory]
    [InlineData("dev/one", new[] { "dev", "one" })]
    [InlineData("/one/two/three", new[] { "one", "two", "three" })]
    [InlineData("/one/../three/", new[] { "one", "..", "three/" })]
    public void Split(string path, string[] expected)
    {
        StoragePath.Split(path).ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ToString_ReturnsFullPath()
    {
        new StoragePath("/test/one/two").ToString().ShouldBe("/test/one/two");
    }

    [Fact]
    public void Equality()
    {
        var path1 = new StoragePath("/test/one/two");
        var path2 = new StoragePath(path1.Full);
        var path3 = new StoragePath("/test/one");

        path1.ShouldBe(path2);
        path1.ShouldNotBe(path3);
    }
}
