namespace Centeva.ObjectStorage.UnitTests;

public class StorageEntryTests
{
    [Fact]
    public void Constructor_SetsPath()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        Assert.Equal(path, entry.Path.Full);
    }

    [Fact]
    public void SetPath_SetsPath()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var newPath = "/new/path/to/file.txt";
        entry.SetPath(newPath);

        Assert.Equal(newPath, entry.Path.Full);
    }

    [Fact]
    public void Name_ReturnsName()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        Assert.Equal("file.txt", entry.Name);
    }

    [Fact]
    public void CreationTime_ReturnsCreationTime()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var time = DateTimeOffset.Now;
        entry.CreationTime = time;

        Assert.Equal(time, entry.CreationTime);
    }

    [Fact]
    public void LastModificationTime_ReturnsLastModificationTime()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var time = DateTimeOffset.Now;
        entry.LastModificationTime = time;

        Assert.Equal(time, entry.LastModificationTime);
    }

    [Fact]
    public void SizeInBytes_ReturnsSizeInBytes()
    {
        var path = "/path/to/file.txt";
        var entry = new StorageEntry(path);

        var size = 1234;
        entry.SizeInBytes = size;

        Assert.Equal(size, entry.SizeInBytes);
    }
}
