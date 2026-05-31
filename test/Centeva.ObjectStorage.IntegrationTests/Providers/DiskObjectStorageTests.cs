using Centeva.ObjectStorage.Builtin;
using System.Text;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class DiskObjectStorageFixture : ObjectStorageFixture
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        Directory.CreateDirectory(_tempDir);
        return new DiskObjectStorage(_tempDir);
    }

    public override void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}

public class DiskObjectStorageTests : CommonObjectStorageTests, IClassFixture<DiskObjectStorageFixture>
{
    public DiskObjectStorageTests(DiskObjectStorageFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAsync_WithFolderPath_RetrievesStorageEntry()
    {
        var path = RandomStoragePath("stat");
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)), cancellationToken: CancellationToken);

        var folderPath = new StoragePath(path.Folder);
        var entry = await _sut.GetAsync(folderPath, CancellationToken);

        entry.Should().NotBeNull();
        entry!.Path.Full.Should().Be(folderPath);
        entry.Path.IsFolder.Should().BeTrue();
        entry.CreationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entry.LastModificationTime!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entry.SizeInBytes.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ContentType()
    {
        var path = await WriteToRandomPathAsync();

        var entry = await _sut.GetAsync(path, CancellationToken);

        entry.Should().NotBeNull();
        entry!.ContentType.Should().BeNull();
    }
}
