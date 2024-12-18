using Centeva.ObjectStorage.AWS;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AwsS3ObjectStorageFixture : ObjectStorageFixture
{
    public AwsS3ObjectStorageFixture() : base("/" + nameof(AwsS3ObjectStorageTests) + "/")
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
    private readonly AwsS3ObjectStorageFixture _fixture;
    public AwsS3ObjectStorageTests(AwsS3ObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    // Test that AwsS3ObjectStorage implements ISupportsMetadata
    [Fact]
    public void AwsS3ObjectStorageImplementsISupportsMetadata()
    {
        var storage = (AwsS3ObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsMetadata>(storage);
    }

    // Test that UpdateMetadataAsync works
    [Fact]
    public async Task UpdateMetadataAsyncWorks()
    {
        var storage = (AwsS3ObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        string path = await WriteToRandomPathAsync();
        var metadata = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        await storage.UpdateMetadataAsync(path, new() { Metadata = metadata });

        var updatedMetadata = await storage.GetAsync(path);
        Assert.NotNull(updatedMetadata);
        Assert.Equal(metadata, updatedMetadata.Metadata);
    }

    // Test that AwsS3ObjectStorage implements ISupportsSignedUrls
    [Fact]
    public void AwsS3ObjectStorageImplementsISupportsSignedUrls()
    {
        var storage = (AwsS3ObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsSignedUrls>(storage);
    }

    [Fact]
    public async void GetAsync_ContentType()
    {
        var storage = (AwsS3ObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var contentType = "application/json";
        var path = await WriteToRandomPath_WithContentTypeAsync(contentType);


        var entry = await storage.GetAsync(path);

        Assert.Equal(contentType, entry.ContentType);
    }
}
