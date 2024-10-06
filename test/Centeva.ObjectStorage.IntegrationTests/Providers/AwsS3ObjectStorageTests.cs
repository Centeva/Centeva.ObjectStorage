using Centeva.ObjectStorage.AWS;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AwsS3ObjectStorageFixture : ObjectStorageFixture
{
    public AwsS3ObjectStorageFixture() : base(nameof(AwsS3ObjectStorageTests))
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new AwsS3ObjectStorage(settings.AwsTestBucketName!, settings.AwsTestBucketRegion!, null, settings.AwsAccessKey!, settings.AwsSecretKey!);
    }

    public override void Cleanup()
    {
    }
}

public class AwsS3ObjectStorageTests : CommonObjectStorageTests, IClassFixture<AwsS3ObjectStorageFixture>
{
    public AwsS3ObjectStorageTests(AwsS3ObjectStorageFixture fixture) : base(fixture)
    {
    }
}
