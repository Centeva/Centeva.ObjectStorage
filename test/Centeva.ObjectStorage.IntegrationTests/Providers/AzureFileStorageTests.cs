using System.Net.Http;
using System.Net.Mime;

using Centeva.ObjectStorage.Azure.File;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AzureFileShareObjectStorageFixture : ObjectStorageFixture
{
    public AzureFileShareObjectStorageFixture() : base("/" + nameof(AzureFileStorageTests) + "/")
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new AzureFileStorage(settings.AzureAccountName!, settings.AzureAccountKey!, settings.AzureBlobStorageTestContainerName!);
    }

    public override void Cleanup()
    {
    }
}

public class AzureFileStorageTests : CommonObjectStorageTests, IClassFixture<AzureFileShareObjectStorageFixture>
{
    private readonly AzureFileShareObjectStorageFixture _fixture;

    public AzureFileStorageTests(AzureFileShareObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AzureFileShareObjectStorageImplementsISupportsMetadata()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsMetadata>(storage);
    }

    [Fact]
    public async Task UpdateMetadataAsyncWorks()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
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
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        Assert.IsAssignableFrom<ISupportsSignedUrls>(storage);
    }

    [Fact]
    public async Task GetAsync_ContentType()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions { ContentType = "application/json" };
        var path = await WriteToRandomPathAsync("", ".json", options);

        var entry = await storage.GetAsync(path);

        entry.ShouldNotBeNull();
        entry!.ContentType.ShouldBe(options.ContentType);
    }

    [Fact]
    public async Task WriteAsync_WithContentDisposition_SetsHeaderWhenRetrieving()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions
        {
            ContentType = "application/json",
            ContentDisposition = new ContentDisposition { FileName = "somefile.json" }
        };
        var path = await WriteToRandomPathAsync("", ".json", options);

        var signedUrl = await storage.GetDownloadUrlAsync(path, null);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl);
        response.EnsureSuccessStatusCode();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.ShouldNotBeNull();
        contentDisposition.FileName.ShouldBe(options.ContentDisposition.FileName);
        contentDisposition.DispositionType.ShouldBe(options.ContentDisposition.DispositionType);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ReturnsValidUrl()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        var path = await WriteToRandomPathAsync("", ".json");
        var signedUrl = await storage.GetDownloadUrlAsync(path, options: null);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.ShouldBe(_testFileContent);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_WithContentDisposition_SetsHeaderWhenRetrieving()
    {
        var storage = (AzureFileStorage)_fixture.CreateStorage(TestSettings.Instance);
        var path = await WriteToRandomPathAsync("", ".json");
        var options = new SignedUrlOptions
        {
            ContentDisposition = new ContentDisposition { FileName = "somefile.json" }
        };
        var signedUrl = await storage.GetDownloadUrlAsync(path, options);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl);
        response.EnsureSuccessStatusCode();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.ShouldNotBeNull();
        contentDisposition.FileName.ShouldBe(options.ContentDisposition.FileName);
        contentDisposition.DispositionType.ShouldBe(options.ContentDisposition.DispositionType);
    }
}
