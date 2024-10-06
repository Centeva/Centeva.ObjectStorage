using System.Text;

using Centeva.ObjectStorage.Builtin;

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
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(_testFileContent)));

        var folderPath = new StoragePath(path.Folder);
        var entry = await _sut.GetAsync(folderPath);

        entry.Should().NotBeNull();
        entry!.Path.Full.Should().Be(folderPath);
        entry.Path.IsFolder.Should().BeTrue();
        entry.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entry.LastModificationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entry.SizeInBytes.Should().BeNull();
    }

}
