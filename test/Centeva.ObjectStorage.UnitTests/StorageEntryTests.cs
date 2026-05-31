namespace Centeva.ObjectStorage.UnitTests;

public class StorageEntryTests
{
    [Fact]
    public void Constructor_SetsPath()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        entry.Path.Full.Should().Be(path);
    }

    [Fact]
    public void SetPath_SetsPath()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var newPath = "/new/path/to/file.txt";
        entry.SetPath(newPath);

        entry.Path.Full.Should().Be(newPath);
    }

    [Fact]
    public void Name_ReturnsName()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        entry.Name.Should().Be("file.txt");
    }

    [Fact]
    public void CreationTime_ReturnsCreationTime()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var time = DateTimeOffset.Now;
        entry.CreationTime = time;

        entry.CreationTime.Should().Be(time);
    }

    [Fact]
    public void LastModificationTime_ReturnsLastModificationTime()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var time = DateTimeOffset.Now;
        entry.LastModificationTime = time;

        entry.LastModificationTime.Should().Be(time);
    }

    [Fact]
    public void SizeInBytes_ReturnsSizeInBytes()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var size = 1234;
        entry.SizeInBytes = size;

        entry.SizeInBytes.Should().Be(size);
    }
}
