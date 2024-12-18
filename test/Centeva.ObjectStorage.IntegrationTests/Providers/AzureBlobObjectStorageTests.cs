using Centeva.ObjectStorage.Azure.Blob;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AzureBlobObjectStorageFixture : ObjectStorageFixture
{
    public AzureBlobObjectStorageFixture() : base("/" + nameof(AzureBlobObjectStorageTests) + "/")
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new AzureBlobObjectStorage(settings.AzureAccountName!, settings.AzureAccountKey!, settings.AzureBlobStorageTestContainerName!);
    }

    public override void Cleanup()
    {
    }
}

public class AzureBlobObjectStorageTests : CommonObjectStorageTests, IClassFixture<AzureBlobObjectStorageFixture>
{
    private readonly AzureBlobObjectStorageFixture _fixture;

    public AzureBlobObjectStorageTests(AzureBlobObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    // Test that azureBlob implements ISupportsMetadata
    [Fact]
    public void AzureBlobObjectStorageImplementsISupportsMetadata()
    {
        var storage = (AzureBlobObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsMetadata>(storage);
    }

    // Test that UpdateMetadataAsync works
    [Fact]
    public async Task UpdateMetadataAsyncWorks()
    {
        var storage = (AzureBlobObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
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

    // Test that azureBlob implements ISupportsSignedUrls
    [Fact]
    public void AzureBlobObjectStorageImplementsISupportsSignedUrls()
    {
        var storage = (AzureBlobObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsSignedUrls>(storage);
    }


    [Fact]
    public async Task GetAsync_ContentType()
    {
        var storage = (AzureBlobObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var contentType = "application/json";
        var path = await WriteToRandomPathAsync("", ".json", contentType);

        var entry = await storage.GetAsync(path);

        entry.Should().NotBeNull();
        entry!.ContentType.Should().Be(contentType);
    }
}
