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

public class DiskObjectStorageTests : ObjectStorageTest, IClassFixture<DiskObjectStorageFixture>
{
    public DiskObjectStorageTests(DiskObjectStorageFixture fixture) : base(fixture)
    {
    }
}
