using System.Text;

namespace Centeva.ObjectStorage.IntegrationTests;

public abstract class ObjectStorageTest
{
    private readonly IObjectStorage _sut;
    private readonly string? _storagePathPrefix;
    private readonly string _testFileContent = $"Hello, World! {Guid.NewGuid()}";

    public ObjectStorageTest(ObjectStorageFixture fixture)
    {
        _sut = fixture.Storage;
        _storagePathPrefix = fixture.StoragePathPrefix;
    }

    [InlineData("")]
    [InlineData("test/test/test")]
    [Theory]
    public async Task Write_SucceedsAndIsReadable(string pathPrefix)
    {
        var path = RandomStoragePath(pathPrefix);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task Write_WithFolderPath_SucceedsAndIsReadable()
    {
        var path = RandomStoragePath("test", extension: "") + StoragePath.PathSeparator;
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        await using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task Write_CollapsesParentPathReferences()
    {
        string path = RandomStoragePath();
        await _sut.WriteAsync(StoragePath.Combine("..", path), new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task Read_CollapsesParentPathReferences()
    {
        string path = RandomStoragePath();
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(StoragePath.Combine("..", path));
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task Read_ReturnsNullForNonexistentObject()
    {
        string path = RandomStoragePath();

        using var stream = await _sut.OpenReadAsync(path);
        stream.Should().BeNull();
    }

    [Fact]
    public async Task Exists_ReturnsFalseForNonexistentObject()
    {
        string path = RandomStoragePath();

        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task Exists_ReturnsTrueForExistingObject()
    {
        string path = RandomStoragePath();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).Should().BeTrue();
    }

    [Fact]
    public async Task Delete_RemovesExistingObject()
    {
        string path = RandomStoragePath();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).Should().BeTrue();

        await _sut.DeleteAsync(path);
        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_DoesNotThrowForNonexistentObject()
    {
        string path = RandomStoragePath();

        await _sut.DeleteAsync(path);
    }

    [Fact(Skip = "Until we can do some cleanup before each test, we can't guarantee that the storage is empty")]
    public async Task List_ReturnsEmptyEnumerableForEmptyStorage()
    {
        var list = await _sut.ListAsync();
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task List_ReturnsKnownObjects()
    {
        // TODO: Until we can do some cleanup before each test, we can't guarantee that the storage is empty
        string path1 = RandomStoragePath();
        string path2 = RandomStoragePath("folder");
        string path3 = RandomStoragePath("folder/morefolder");

        await _sut.WriteAsync(path1, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        await _sut.WriteAsync(path2, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        await _sut.WriteAsync(path3, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        var list = await _sut.ListAsync();
        list.Should().Contain(path1);
        list.Should().Contain(path2);
        list.Should().Contain(path3);
    }

    [Fact]
    public async Task Rename_RenamesObject()
    {
        // Arrange
        string originalName = RandomStoragePath();
        string newName = RandomStoragePath();

        // Write an object with the original name
        await _sut.WriteAsync(originalName, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        // Act
        await _sut.RenameAsync(originalName, newName);

        // Assert
        // Check that the original object no longer exists
        (await _sut.ExistsAsync(originalName)).Should().BeFalse();

        // Check that the new object exists
        (await _sut.ExistsAsync(newName)).Should().BeTrue();

        // Check that the content of the new object is the same as the original content
        using var stream = await _sut.OpenReadAsync(newName);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }


    private StoragePath RandomStoragePath(string subPath = "", string extension = ".txt")
        => new(StoragePath.Combine(_storagePathPrefix ?? "", subPath, Guid.NewGuid().ToString() + extension));
}
