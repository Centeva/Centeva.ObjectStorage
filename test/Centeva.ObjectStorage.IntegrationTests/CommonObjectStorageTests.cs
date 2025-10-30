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

        using var stream = await _sut.OpenReadAsync(path);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task WriteAsync_WithFolderPath_SucceedsAndIsReadable()
    {
        var path = RandomStoragePath("test", extension: "") + StoragePath.PathSeparator;
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(path);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task WriteAsync_CollapsesParentPathReferences()
    {
        string path = RandomStoragePath();
        await _sut.WriteAsync(StoragePath.Combine("..", path), new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(path);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_CollapsesParentPathReferences()
    {
        string path = await WriteToRandomPathAsync();

        using var stream = await _sut.OpenReadAsync(StoragePath.Combine("..", path));
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsNullForNonexistentObject()
    {
        string path = RandomStoragePath();

        using var stream = await _sut.OpenReadAsync(path);
        stream.ShouldBeNull();
    }

    [Fact]
    public async Task ExistAsync_ReturnsFalseForNonexistentObject()
    {
        string path = RandomStoragePath();

        (await _sut.ExistsAsync(path)).ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        (await _sut.ExistsAsync(path)).ShouldBeTrue();

        await _sut.DeleteAsync(path);
        (await _sut.ExistsAsync(path)).ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForNonexistentObject()
    {
        string path = RandomStoragePath();

        await _sut.DeleteAsync(path);
    }

    [Fact]
    public async Task ListAsync_AllowsNoParams()
    {
        var action = () => _sut.ListAsync();

        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_AllowsNullPath()
    {
        var action = () => _sut.ListAsync(null);

        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_AllowsRootPath()
    {
        var action = () => _sut.ListAsync(StoragePath.RootFolderPath);

        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_WithFilePath_ThrowsArgumentException()
    {
        var action = () => _sut.ListAsync("folder/filePath");

        var ex = await action.ShouldThrowAsync<ArgumentException>();
        ex.ParamName.ShouldBe("path");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyListWhenNoEntriesExist()
    {
        var emptyPath = new StoragePath(Guid.NewGuid() + "/");

        var list = await _sut.ListAsync(emptyPath);

        list.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListAsync_WithPath_ReturnsContainedObjects()
    {
        var path = await WriteToRandomPathAsync();

        var list = (await _sut.ListAsync(path.Folder)).Select(x => x.Path).ToList();
        list.ShouldContain(path);
    }

    [Fact]
    public async Task ListAsync_WithoutRecurseWithFileInFolder_ReturnsFolderOnly()
    {
        var folderName = Guid.NewGuid().ToString();
        var path = await WriteToRandomPathAsync(folderName);

        var list = (await _sut.ListAsync(_storagePathPrefix)).Select(x => x.Path).ToList();

        list.ShouldContain(new StoragePath(path.Folder));
        list.ShouldNotContain(path);
    }

    [Fact]
    public async Task ListAsync_WithRecurseWithFileInFolder_ReturnsFolderAndFile()
    {
        var folderName = Guid.NewGuid().ToString();
        var path = await WriteToRandomPathAsync(folderName);

        var list = (await _sut.ListAsync(_storagePathPrefix, new ListOptions { Recurse = true })).Select(x => x.Path).ToList();

        list.ShouldContain(new StoragePath(path.Folder));
        list.ShouldContain(path);
    }


    [Fact]
    public async Task ListAsync_IncludesFileMetadata()
    {
        var path = await WriteToRandomPathAsync(Guid.NewGuid().ToString());

        var list = await _sut.ListAsync(path.Folder);

        var entry = list.FirstOrDefault(x => x.Path.Equals(path));
        entry.ShouldNotBeNull();
        entry!.CreationTime!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        entry.LastModificationTime!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        entry.SizeInBytes.ShouldBe(_testFileContent.Length);
    }

    [Fact]
    public async Task ListAsync_LotsOfFiles()
    {
        var currentCount = (await _sut.ListAsync(_storagePathPrefix)).Count;

        var entriesToCreate = 5_000 - currentCount;

        for (var i = 0; i < entriesToCreate; i++)
        {
            await WriteToRandomPathAsync();
        }

        var entries = await _sut.ListAsync(_storagePathPrefix);

        entries.Count.ShouldBeGreaterThanOrEqualTo(5_000);
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
        (await _sut.ExistsAsync(originalPath)).ShouldBeFalse();

        // Check that the new object exists
        (await _sut.ExistsAsync(newPath)).ShouldBeTrue();

        // Check that the content of the new object is the same as the original content
        using var stream = await _sut.OpenReadAsync(newPath);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task GetAsync_RetrievesStorageEntry()
    {
        string path = await WriteToRandomPathAsync();

        var entry = await _sut.GetAsync(path);

        entry.ShouldNotBeNull();
        entry!.Path.Full.ShouldBe(path);
        entry.CreationTime!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.LastModificationTime!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.SizeInBytes.ShouldBe(_testFileContent.Length);
    }

    [Fact]
    public async Task GetAsync_WithMissingEntry_ReturnsNull()
    {
        string path = RandomStoragePath();

        var entry = await _sut.GetAsync(path);
        entry.ShouldBeNull();
    }

    [Fact]
    public async Task CopyAsync_CopiesObject()
    {
        var sourcePath = await WriteToRandomPathAsync("source");
        StoragePath targetPath = RandomStoragePath("target").Folder;
        await _sut.CopyAsync(sourcePath, _sut, targetPath);

        StoragePath newFilePath = StoragePath.Combine(targetPath.Full, sourcePath.Name);
        using var stream = await _sut.OpenReadAsync(newFilePath);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task CopyAllAsync_CopiesAllObjectsRecursively()
    {
        StoragePath sourcePath = RandomStoragePath("source", "") + StoragePath.PathSeparator;
        await WriteToRandomPathAsync(sourcePath);
        await WriteToRandomPathAsync(sourcePath);
        await WriteToRandomPathAsync(StoragePath.Combine(sourcePath, "subpath"));

        StoragePath targetPath = RandomStoragePath("target", "") + StoragePath.PathSeparator;
        await _sut.CopyAllAsync(sourcePath, _sut, targetPath);

        var sourceObjects = await _sut.ListAsync(sourcePath, new ListOptions { Recurse = true });
        var targetObjects = await _sut.ListAsync(targetPath, new ListOptions { Recurse = true });

        var sourceObjectsWithoutPath = sourceObjects.Where(x => x.Path.IsFile).Select(x => x.Path.Full.Substring(sourcePath.Full.Length)).ToList();
        var targetObjectsWithoutPath = targetObjects.Where(x => x.Path.IsFile).Select(x => x.Path.Full.Substring(targetPath.Full.Length)).ToList();
        targetObjectsWithoutPath.ShouldBeEquivalentTo(sourceObjectsWithoutPath);
    }

    protected async Task<StoragePath> WriteToRandomPathAsync(string subPath = "", string extension = ".txt", WriteOptions? options = null)
    {
        var path = RandomStoragePath(subPath, extension);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), options);

        return path;
    }


    protected StoragePath RandomStoragePath(string subPath = "", string extension = ".txt")
    {
        var path = StoragePath.Combine(subPath, Guid.NewGuid() + extension);

        if (_storagePathPrefix is not null && !path.StartsWith(_storagePathPrefix))
        {
            path = StoragePath.Combine(_storagePathPrefix, path);
        }

        return path;
    }
}
