namespace Centeva.ObjectStorage.UnitTests;

public class BlobTests
{
    [Fact]
    public void Constructor_CreatesWithGivenName()
    {
        var blob = new Blob("folder/file.txt");

        blob.Name.Should().Be("folder/file.txt");
    }

    [Fact]
    public void Constructor_NormalizesName()
    {
        var blob = new Blob("/folder/file.txt");

        blob.Name.Should().Be("folder/file.txt");
    }

    [Fact]
    public void Constructor_SetsFilename()
    {
        var blob = new Blob("folder/file.txt");

        blob.Filename.Should().Be("file.txt");
    }

    [Fact]
    public void Constructor_SetsFolderPath()
    {
        var blob = new Blob("folder/file.txt");

        blob.FolderPath.Should().Be("/folder");
    }

    [Fact]
    public void Constructor_SetsFolderPathToRootIfNoFolder()
    {
        var blob = new Blob("file.txt");

        blob.FolderPath.Should().Be("/");
    }

    [Fact]
    public void FullPath_IncludesLeadingSeparator()
    {
        var blob = new Blob("folder/file.txt");

        blob.FullPath.Should().Be("/folder/file.txt");
    }

    [Fact]
    public void FullPath_IncludesLeadingSeparatorForRoot()
    {
        var blob = new Blob("file.txt");

        blob.FullPath.Should().Be("/file.txt");
    }
}
