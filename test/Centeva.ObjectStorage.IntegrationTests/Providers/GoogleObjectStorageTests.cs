using System.Net.Http;
using System.Net.Mime;

using Centeva.ObjectStorage.GCP;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class GoogleObjectStorageFixture : ObjectStorageFixture
{
    public GoogleObjectStorageFixture() : base("/" + nameof(GoogleObjectStorageTests) + "/")
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
    private readonly GoogleObjectStorageFixture _fixture;
    public GoogleObjectStorageTests(GoogleObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAsync_ContentType()
    {
        var storage = _fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions { ContentType = "application/json" };
        var path = await WriteToRandomPathAsync("", ".json", options);

        var entry = await storage.GetAsync(path);

        entry.ShouldNotBeNull();
        entry!.ContentType.ShouldBe(options.ContentType);
    }

    [Fact]
    public async Task WriteAsync_WithContentDisposition_SetsHeaderWhenRetrieving()
    {
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions
        {
            ContentType = "application/json",
            ContentDisposition = new ContentDisposition {FileName = "somefile.json"}
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
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
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
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
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

    [Fact]
    public async Task CopyAsync_CopiesObjectWithMetadata()
    {
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);

        var metadata = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } });
        var options = new WriteOptions("text/plain", metadata);
        var sourcePath = await WriteToRandomPathAsync("source", options: options);
        var targetPath = new StoragePath("target" + StoragePath.PathSeparator);
        if (_storagePathPrefix is not null)
            targetPath = StoragePath.Combine(_storagePathPrefix, targetPath) + StoragePath.PathSeparator;

        await storage.CopyAsync(sourcePath, storage, targetPath);

        StoragePath newFilePath = StoragePath.Combine(targetPath.Full, sourcePath.Name);
        var entry = await storage.GetAsync(newFilePath);
        entry.ShouldNotBeNull();
        entry!.ContentType.ShouldBe(options.ContentType);
        entry!.Metadata.ShouldBeEquivalentTo(metadata);
    }
}
