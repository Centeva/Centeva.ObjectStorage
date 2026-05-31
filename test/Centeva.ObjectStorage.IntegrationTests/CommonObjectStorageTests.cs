using System.Text;

#pragma warning disable xUnit1051 // ReadToEndAsync does not support CancellationToken on .NET Framework

namespace Centeva.ObjectStorage.IntegrationTests;

public abstract class CommonObjectStorageTests
{
    protected readonly IObjectStorage _sut;
    private readonly string? _storagePathPrefix;
    protected readonly string _testFileContent = $"Hello, World! {Guid.NewGuid()}";
    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

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
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), cancellationToken: CancellationToken);

        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task WriteAsync_WithFolderPath_SucceedsAndIsReadable()
    {
        var path = RandomStoragePath("test", extension: "") + StoragePath.PathSeparator;
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), cancellationToken: CancellationToken);

        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task WriteAsync_CollapsesParentPathReferences()
    {
        string path = RandomStoragePath();
        await _sut.WriteAsync(StoragePath.Combine("..", path), new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), cancellationToken: CancellationToken);

        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_CollapsesParentPathReferences()
    {
        string path = await WriteToRandomPathAsync();

        using var stream = await _sut.OpenReadAsync(StoragePath.Combine("..", path), CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsNullForNonexistentObject()
    {
        string path = RandomStoragePath();

        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        stream.Should().BeNull();
    }

    [Fact]
    public async Task ExistAsync_ReturnsFalseForNonexistentObject()
    {
        string path = RandomStoragePath();

        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), cancellationToken: CancellationToken);
        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingObject()
    {
        string path = await WriteToRandomPathAsync();

        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeTrue();

        await _sut.DeleteAsync(path, CancellationToken);
        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForNonexistentObject()
    {
        string path = RandomStoragePath();

        await _sut.DeleteAsync(path, CancellationToken);
    }

    [Fact]
    public async Task ListAsync_AllowsNoParams()
    {
        var action = () => _sut.ListAsync();

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_AllowsNullPath()
    {
        var action = () => _sut.ListAsync(null);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_AllowsRootPath()
    {
        var action = () => _sut.ListAsync(StoragePath.RootFolderPath);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_WithFilePath_ThrowsArgumentException()
    {
        var action = () => _sut.ListAsync("folder/filePath");

        var ex = await action.Should().ThrowAsync<ArgumentException>();
        ex.Which.ParamName.Should().Be("path");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyListWhenNoEntriesExist()
    {
        var emptyPath = new StoragePath(Guid.NewGuid() + "/");

        var list = await _sut.ListAsync(emptyPath, cancellationToken: CancellationToken);

        list.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_WithPath_ReturnsContainedObjects()
    {
        var path = await WriteToRandomPathAsync();

        var list = (await _sut.ListAsync(path.Folder, cancellationToken: CancellationToken)).Select(x => x.Path).ToList();
        list.Should().Contain(path);
    }

    [Fact]
    public async Task ListAsync_WithoutRecurseWithFileInFolder_ReturnsFolderOnly()
    {
        var folderName = Guid.NewGuid().ToString();
        var path = await WriteToRandomPathAsync(folderName);

        var list = (await _sut.ListAsync(_storagePathPrefix, cancellationToken: CancellationToken)).Select(x => x.Path).ToList();

        list.Should().Contain(new StoragePath(path.Folder));
        list.Should().NotContain(path);
    }

    [Fact]
    public async Task ListAsync_WithRecurseWithFileInFolder_ReturnsFolderAndFile()
    {
        var folderName = Guid.NewGuid().ToString();
        var path = await WriteToRandomPathAsync(folderName);

        var list = (await _sut.ListAsync(_storagePathPrefix, new ListOptions { Recurse = true }, CancellationToken)).Select(x => x.Path).ToList();

        list.Should().Contain(new StoragePath(path.Folder));
        list.Should().Contain(path);
    }


    [Fact]
    public async Task ListAsync_IncludesFileMetadata()
    {
        var path = await WriteToRandomPathAsync(Guid.NewGuid().ToString());

        var list = await _sut.ListAsync(path.Folder, cancellationToken: CancellationToken);

        var entry = list.FirstOrDefault(x => x.Path.Equals(path));
        entry.Should().NotBeNull();
        entry!.CreationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
        entry.LastModificationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
        entry.SizeInBytes.Should().Be(_testFileContent.Length);
    }

    [Fact]
    public async Task ListAsync_LotsOfFiles()
    {
        var currentCount = (await _sut.ListAsync(_storagePathPrefix, cancellationToken: CancellationToken)).Count;

        var entriesToCreate = 5_000 - currentCount;

        for (var i = 0; i < entriesToCreate; i++)
        {
            await WriteToRandomPathAsync();
        }

        var entries = await _sut.ListAsync(_storagePathPrefix, cancellationToken: CancellationToken);

        entries.Count.Should().BeGreaterThanOrEqualTo(5_000);
    }

    [Fact]
    public async Task RenameAsync_RenamesObject()
    {
        // Arrange
        var originalPath = await WriteToRandomPathAsync();
        var newPath = RandomStoragePath();

        // Act
        await _sut.RenameAsync(originalPath, newPath, CancellationToken);

        // Assert
        // Check that the original object no longer exists
        (await _sut.ExistsAsync(originalPath, CancellationToken)).Should().BeFalse();

        // Check that the new object exists
        (await _sut.ExistsAsync(newPath, CancellationToken)).Should().BeTrue();

        // Check that the content of the new object is the same as the original content
        using var stream = await _sut.OpenReadAsync(newPath, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task RenameAsync_WithExistingDestination_OverwritesDestination()
    {
        // Arrange
        var originalPath = await WriteToRandomPathAsync();
        var destinationPath = RandomStoragePath();

        // Create a copy at the destination path first
        var differentContent = "Different content that should be overwritten.";
        await _sut.WriteAsync(destinationPath, new MemoryStream(Encoding.UTF8.GetBytes(differentContent)), cancellationToken: CancellationToken);

        // Verify both files exist before rename
        (await _sut.ExistsAsync(originalPath, CancellationToken)).Should().BeTrue();
        (await _sut.ExistsAsync(destinationPath, CancellationToken)).Should().BeTrue();

        // Act
        await _sut.RenameAsync(originalPath, destinationPath, CancellationToken);

        // Assert
        // Check that the original object no longer exists
        (await _sut.ExistsAsync(originalPath, CancellationToken)).Should().BeFalse();

        // Check that the destination object still exists
        (await _sut.ExistsAsync(destinationPath, CancellationToken)).Should().BeTrue();

        // Check that the destination content matches the original content
        using var stream = await _sut.OpenReadAsync(destinationPath, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task GetAsync_RetrievesStorageEntry()
    {
        string path = await WriteToRandomPathAsync();

        var entry = await _sut.GetAsync(path, CancellationToken);

        entry.Should().NotBeNull();
        entry!.Path.Full.Should().Be(path);
        entry.CreationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entry.LastModificationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entry.SizeInBytes.Should().Be(_testFileContent.Length);
    }

    [Fact]
    public async Task GetAsync_WithMissingEntry_ReturnsNull()
    {
        string path = RandomStoragePath();

        var entry = await _sut.GetAsync(path, CancellationToken);
        entry.Should().BeNull();
    }

    [Fact]
    public async Task CopyAsync_CopiesObject()
    {
        var sourcePath = await WriteToRandomPathAsync("source");
        StoragePath targetPath = RandomStoragePath("target").Folder;
        await _sut.CopyAsync(sourcePath, _sut, targetPath, CancellationToken);

        StoragePath newFilePath = StoragePath.Combine(targetPath.Full, sourcePath.Name);
        using var stream = await _sut.OpenReadAsync(newFilePath, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task CopyAllAsync_CopiesAllObjectsRecursively()
    {
        StoragePath sourcePath = RandomStoragePath("source", "") + StoragePath.PathSeparator;
        await WriteToRandomPathAsync(sourcePath);
        await WriteToRandomPathAsync(sourcePath);
        await WriteToRandomPathAsync(StoragePath.Combine(sourcePath, "subpath"));

        StoragePath targetPath = RandomStoragePath("target", "") + StoragePath.PathSeparator;
        await _sut.CopyAllAsync(sourcePath, _sut, targetPath, CancellationToken);

        var sourceObjects = await _sut.ListAsync(sourcePath, new ListOptions { Recurse = true }, CancellationToken);
        var targetObjects = await _sut.ListAsync(targetPath, new ListOptions { Recurse = true }, CancellationToken);

        var sourceObjectsWithoutPath = sourceObjects.Where(x => x.Path.IsFile).Select(x => x.Path.Full.Substring(sourcePath.Full.Length)).ToList();
        var targetObjectsWithoutPath = targetObjects.Where(x => x.Path.IsFile).Select(x => x.Path.Full.Substring(targetPath.Full.Length)).ToList();
        targetObjectsWithoutPath.Should().BeEquivalentTo(sourceObjectsWithoutPath);
    }

    protected async Task<StoragePath> WriteToRandomPathAsync(string subPath = "", string extension = ".txt", WriteOptions? options = null)
    {
        var path = RandomStoragePath(subPath, extension);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), options, CancellationToken);

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
