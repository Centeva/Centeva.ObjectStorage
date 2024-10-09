using Centeva.ObjectStorage.GCP;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class GoogleObjectStorageFixture : ObjectStorageFixture
{
    public GoogleObjectStorageFixture() : base(nameof(GoogleObjectStorageTests))
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return GoogleObjectStorage.CreateFromCredentialsJson(settings.GoogleStorageTestBucketName!, settings.GoogleStorageCredentialsJson!);
    }

    public override void Cleanup()
    {
    }
}

public class GoogleObjectStorageTests : CommonObjectStorageTests, IClassFixture<GoogleObjectStorageFixture>
{
    public GoogleObjectStorageTests(GoogleObjectStorageFixture fixture) : base(fixture)
    {
    }
}
