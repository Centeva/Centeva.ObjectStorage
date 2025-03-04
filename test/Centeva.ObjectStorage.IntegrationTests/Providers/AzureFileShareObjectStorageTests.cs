using Centeva.ObjectStorage.Azure.FileShare;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AzureFileShareObjectStorageFixture : ObjectStorageFixture
{
    public AzureFileShareObjectStorageFixture() : base("/" + nameof(AzureFileShareObjectStorageTests) + "/")
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new AzureFileShareObjectStorage(settings.AzureAccountName!, settings.AzureAccountKey!, settings.AzureStorageTestContainerName!);
    }

    public override void Cleanup()
    {
    }
}

public class AzureFileShareObjectStorageTests : CommonObjectStorageTests, IClassFixture<AzureFileShareObjectStorageFixture>
{
    private readonly AzureFileShareObjectStorageFixture _fixture;

    public AzureFileShareObjectStorageTests(AzureFileShareObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AzureFileShareObjectStorageImplementsISupportsMetadata()
    {
        var storage = (AzureFileShareObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsMetadata>(storage);
    }

    [Fact]
    public async Task UpdateMetadataAsyncWorks()
    {
        var storage = (AzureFileShareObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var path = await WriteToRandomPathAsync();
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

    [Fact]
    public void AzureFileShareObjectStorageImplementsISupportsSignedUrls()
    {
        var storage = (AzureFileShareObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsSignedUrls>(storage);
    }

    [Fact]
    public async Task GetAsync_ContentType()
    {
        var storage = (AzureFileShareObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions("application/json", null);
        var path = await WriteToRandomPathAsync("", ".json", options);

        var entry = await storage.GetAsync(path);

        entry.ShouldNotBeNull();
        entry!.ContentType.ShouldBe(options.ContentType);
    }
}
