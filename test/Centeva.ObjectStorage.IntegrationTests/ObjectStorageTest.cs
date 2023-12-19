using System.Text;

namespace Centeva.ObjectStorage.IntegrationTests;

public abstract class ObjectStorageTest
{
    private readonly IObjectStorage _sut;
    private readonly string? _objectNamePrefix;
    private readonly string _testFileContent = $"Hello, World! {Guid.NewGuid()}";

    public ObjectStorageTest(ObjectStorageFixture fixture)
    {
        _sut = fixture.Storage;
        _objectNamePrefix = fixture.ObjectNamePrefix;
    }

    [InlineData("")]
    [InlineData("test/test/test")]
    [Theory]
    public async Task Write_SucceedsAndIsReadable(string pathPrefix)
    {
        var path = RandomObjectName(pathPrefix);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        using var stream = await _sut.OpenReadAsync(path);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task Write_CollapsesParentPathReferences()
    {
        string path = RandomObjectName();
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
        string path = RandomObjectName();
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
        string path = RandomObjectName();

        using var stream = await _sut.OpenReadAsync(path);
        stream.Should().BeNull();
    }

    [Fact]
    public async Task Exists_ReturnsFalseForNonexistentObject()
    {
        string path = RandomObjectName();

        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task Exists_ReturnsTrueForExistingObject()
    {
        string path = RandomObjectName();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).Should().BeTrue();
    }

    [Fact]
    public async Task Delete_RemovesExistingObject()
    {
        string path = RandomObjectName();

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));
        (await _sut.ExistsAsync(path)).Should().BeTrue();

        await _sut.DeleteAsync(path);
        (await _sut.ExistsAsync(path)).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_DoesNotThrowForNonexistentObject()
    {
        string path = RandomObjectName();

        await _sut.DeleteAsync(path);
    }

    private string RandomObjectName(string subPath = "", string extension = ".txt") => StoragePath.Combine(_objectNamePrefix ?? "", subPath, Guid.NewGuid().ToString() + extension);
}
