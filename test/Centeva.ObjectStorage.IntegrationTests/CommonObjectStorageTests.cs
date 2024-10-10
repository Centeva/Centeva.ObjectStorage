using System.Text;

namespace Centeva.ObjectStorage.IntegrationTests;

public abstract class CommonObjectStorageTests
{
    protected readonly IObjectStorage _sut;
    private readonly string? _storagePathPrefix;
    protected readonly string _testFileContent = $"Hello, World! {Guid.NewGuid()}";

    protected CommonObjectStorageTests(ObjectStorageFixture fixture)
    {
        _sut = fixture.Storage;
        _storagePathPrefix = fixture.StoragePathPrefix;
    }

    [InlineData("")]
    [InlineData("test/test/test")]
    [Theory]
    public async Task WriteAsync_SucceedsAndIsReadable(string pathPrefix)
    {
        var path = RandomStoragePath(pathPrefix);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        await using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task WriteAsync_WithFolderPath_SucceedsAndIsReadable()
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
    public async Task WriteAsync_CollapsesParentPathReferences()
    {
        string path = RandomStoragePath();
        await _sut.WriteAsync(StoragePath.Combine("..", path), new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        await using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_CollapsesParentPathReferences()
    {
        string path = await WriteToRandomPathAsync();

        await using var stream = await _sut.OpenReadAsync(StoragePath.Combine("..", path));
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsNullForNonexistentObject()
    {
        string path = RandomStoragePath();

        await using var stream = await _sut.OpenReadAsync(path);
        stream.Should().BeNull();
    }

    [Fact]
    public async Task ExistAsync_ReturnsFalseForNonexistentObject()
    {
        string path = RandomStoragePath();

        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        (await _sut.ExistsAsync(path)).Should().BeTrue();

        await _sut.DeleteAsync(path);
        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForNonexistentObject()
    {
        string path = RandomStoragePath();

        await _sut.DeleteAsync(path);
    }

    [Fact]
    public async Task ListAsync_AllowsNullPath()
    {
        // We can't really test return value because container could have varying files at this level
        var action = () => _sut.ListAsync(null);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_AllowsRootPath()
    {
        // We can't really test return value because container could have varying files at this level
        var action = () => _sut.ListAsync(StoragePath.RootFolderPath);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_DisallowsFilePaths()
    {
        var action = () => _sut.ListAsync("folder/filePath");

        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("path");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyListWhenNoEntriesExist()
    {
        var emptyPath = RandomStorageFolder("emptyFolder");

        var list = await _sut.ListAsync(emptyPath);

        list.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_WithPath_ReturnsOnlyContainedObjects()
    {
        var path1 = await WriteToRandomPathAsync("listAsync");
        var path2 = await WriteToRandomPathAsync("listAsync");
        var path3 = await WriteToRandomPathAsync("listAsync");

        var list = await _sut.ListAsync(path1.Folder);
        list.Should().Contain(x => x.Path.Equals(path1));
        list.Should().Contain(x => x.Path.Equals(path2));
        list.Should().Contain(x => x.Path.Equals(path3));
    }

    [Fact]
    public async Task ListAsync_IncludesFileMetadata()
    {
        var path = await WriteToRandomPathAsync(Guid.NewGuid().ToString());

        var list = await _sut.ListAsync(path.Folder);

        var entry = list.FirstOrDefault(x => x.Path.Equals(path));
        entry.Should().NotBeNull();
        entry!.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        entry.LastModificationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        entry.SizeInBytes.Should().Be(_testFileContent.Length);
    }

    [Fact]
    public async Task RenameAsync_RenamesObject()
    {
        // Arrange
        var originalPath = await WriteToRandomPathAsync();
        var newPath = RandomStoragePath();

        // Act
        await _sut.RenameAsync(originalPath, newPath);

        // Assert
        // Check that the original object no longer exists
        (await _sut.ExistsAsync(originalPath)).Should().BeFalse();

        // Check that the new object exists
        (await _sut.ExistsAsync(newPath)).Should().BeTrue();

        // Check that the content of the new object is the same as the original content
        await using var stream = await _sut.OpenReadAsync(newPath);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task GetAsync_RetrievesStorageEntry()
    {
        string path = await WriteToRandomPathAsync();

        var entry = await _sut.GetAsync(path);

        entry.Should().NotBeNull();
        entry!.Path.Full.Should().Be(path);
        entry.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.LastModificationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.SizeInBytes.Should().Be(_testFileContent.Length);
    }

    [Fact]
    public async Task GetAsync_WithMissingEntry_ReturnsNull()
    {
        string path = RandomStoragePath();

        var entry = await _sut.GetAsync(path);
        entry.Should().BeNull();
    }

    protected async Task<StoragePath> WriteToRandomPathAsync(string subPath = "", string extension = ".txt")
    {
        var path = RandomStoragePath(subPath, extension);

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        return path;
    }

    protected StoragePath RandomStoragePath(string subPath = "", string extension = ".txt")
        => new(StoragePath.Combine(_storagePathPrefix ?? "", subPath, Guid.NewGuid().ToString() + extension));

    protected StoragePath RandomStorageFolder(string subPath = "")
        => new(StoragePath.Combine(_storagePathPrefix ?? "", subPath, Guid.NewGuid().ToString()) + StoragePath.PathSeparatorString);

}
