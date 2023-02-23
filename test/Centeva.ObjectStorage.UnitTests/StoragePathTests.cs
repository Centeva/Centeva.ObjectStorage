namespace Centeva.ObjectStorage.UnitTests;

public class StoragePathTests
{
    [Theory]
    [InlineData("dev/..", "/")]
    [InlineData("dev/../storage", "/storage")]
    [InlineData("/one", "/one")]
    [InlineData("/one/", "/one")]
    [InlineData("/one/../../../../", "/")]
    public void Normalize(string path, string normalizedPath)
    {
        StoragePath.Normalize(path).Should().Be(normalizedPath);
    }

    [Theory]
    [InlineData("dev/..", "")]
    [InlineData("dev/../storage", "storage")]
    [InlineData("/one", "one")]
    [InlineData("/one/", "one")]
    [InlineData("/one/../../../../", "")]
    public void NormalizeRemoveLeadingSlash(string path, string normalizedPath)
    {
        StoragePath.Normalize(path, true).Should().Be(normalizedPath);
    }

    [Theory]
    [InlineData(new[] { "dev", "one" }, "/dev/one")]
    [InlineData(new[] { "one", "two", "three" }, "/one/two/three")]
    [InlineData(new[] { "one", "..", "three" }, "/three")]
    public void Combine(string[] parts, string expectedPath)
    {
        StoragePath.Combine(parts).Should().Be(expectedPath);
    }
}
